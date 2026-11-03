using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Il2CppSystem;
using UnityEngine;

namespace MoonSweepersMod.Features
{
    /// <summary>
    /// Candidate IL2CPP type / member names. We try each until one resolves, so the
    /// mod keeps working even though we can't see the game's real API at compile time.
    /// Tune these after running the in-game TypeDumper (BepInEx/plugins/.../type_dump.txt).
    /// </summary>
    internal static class GameNames
    {
        public static readonly string[] VehicleTypes = { "Vehicle", "Car", "PlayerVehicle", "MoonCar", "CleanerCar", "Spaceship" };
        public static readonly string[] PlayerTypes = { "Player", "PlayerController", "LocalPlayer", "Astronaut" };
        public static readonly string[] SingletonGetters = { "Instance", "get_Instance", "LocalInstance", "get_LocalPlayer" };
        public static readonly string[] MoveStepMethods = { "MoveForward", "Drive", "Step", "Travel", "Move", "Advance" };
        public static readonly string[] SpawnMethods = { "Spawn", "Instantiate", "Place", "CreatePart", "Build" };
    }

    internal static class Helpers
    {
        public static Il2CppSystem.Type FirstType(params string[] names)
        {
            foreach (var n in names)
            {
                // try full name first, then simple name searched across assemblies
                var t = Core.Reflect.GetType(n);
                if (t != null) return t;
                t = Core.Reflect.FindType(n);
                if (t != null) return t;
            }
            return null;
        }

        public static object FindSingleton(Il2CppSystem.Type type)
        {
            if (type == null) return null;
            foreach (var g in GameNames.SingletonGetters)
            {
                var inst = Core.Reflect.CallStatic(type.FullName, g);
                if (inst != null) return inst;
            }
            // maybe a static field "Instance"
            var f = type.GetField("Instance");
            if (f != null) return f.GetValue(null);
            return null;
        }
    }

    // ------------------------------------------------------------------ Blueprint
    public class BlueprintPart
    {
        public string Prefab = "Crate";
        public float X, Y, Z, RY;
    }

    public class BlueprintPreset
    {
        public string Name = "新预设";
        public List<BlueprintPart> Parts = new List<BlueprintPart>();
    }

    public static class BlueprintSystem
    {
        public static List<BlueprintPreset> Presets = new List<BlueprintPreset>();
        public static string FilePath { get; set; }
        public static string LastPlaceLog = "";

        public static void Init(string path)
        {
            FilePath = path;
            Load();
            if (Presets.Count == 0) SeedDefaults();
        }

        static void SeedDefaults()
        {
            Presets.Add(new BlueprintPreset
            {
                Name = "清洁补给站",
                Parts = new List<BlueprintPart>
                {
                    new BlueprintPart { Prefab = "Crate", X = 0, Y = 0, Z = 0 },
                    new BlueprintPart { Prefab = "Refiner", X = 2, Y = 0, Z = 0 },
                    new BlueprintPart { Prefab = "Lamp", X = 4, Y = 0, Z = 0 }
                }
            });
            Presets.Add(new BlueprintPreset
            {
                Name = "车库",
                Parts = new List<BlueprintPart>
                {
                    new BlueprintPart { Prefab = "Pad", X = 0, Y = 0, Z = 0 },
                    new BlueprintPart { Prefab = "Charger", X = 3, Y = 0, Z = 0 }
                }
            });
            Save();
        }

        public static void Save()
        {
            try
            {
                var sb = new StringBuilder();
                foreach (var p in Presets)
                {
                    sb.AppendLine("PRESET:" + p.Name);
                    foreach (var part in p.Parts)
                        sb.AppendLine($"PART:{part.Prefab}|{part.X}|{part.Y}|{part.Z}|{part.RY}");
                }
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath) ?? ".");
                File.WriteAllText(FilePath, sb.ToString());
            }
            catch (Exception e) { Core.Log.Error("[Blueprint] Save: " + e); }
        }

        public static void Load()
        {
            Presets.Clear();
            try
            {
                if (!File.Exists(FilePath)) return;
                BlueprintPreset cur = null;
                foreach (var line in File.ReadAllLines(FilePath))
                {
                    if (line.StartsWith("PRESET:")) { cur = new BlueprintPreset { Name = line.Substring(7) }; Presets.Add(cur); }
                    else if (line.StartsWith("PART:") && cur != null)
                    {
                        var p = line.Substring(5).Split('|');
                        if (p.Length >= 5)
                            cur.Parts.Add(new BlueprintPart
                            {
                                Prefab = p[0],
                                X = float.Parse(p[1], System.Globalization.CultureInfo.InvariantCulture),
                                Y = float.Parse(p[2], System.Globalization.CultureInfo.InvariantCulture),
                                Z = float.Parse(p[3], System.Globalization.CultureInfo.InvariantCulture),
                                RY = float.Parse(p[4], System.Globalization.CultureInfo.InvariantCulture)
                            });
                    }
                }
            }
            catch (Exception e) { Core.Log.Error("[Blueprint] Load: " + e); }
        }

        public static void AddPreset(BlueprintPreset p) { Presets.Add(p); Save(); }
        public static void RemovePreset(int index) { if (index >= 0 && index < Presets.Count) { Presets.RemoveAt(index); Save(); } }

        /// <summary>Place a preset. Tries the game's spawn/builder API; otherwise logs the intent.</summary>
        public static void Place(BlueprintPreset preset)
        {
            var sb = new StringBuilder();
            var builderType = Helpers.FirstType("BuildManager", "Builder", "ConstructionManager", "BaseBuilder", "Blueprint");
            int spawned = 0;
            foreach (var part in preset.Parts)
            {
                bool ok = false;
                if (builderType != null)
                {
                    foreach (var m in GameNames.SpawnMethods)
                        if (Core.Reflect.CallStatic(builderType.FullName, m, part.Prefab,
                            Core.Reflect.MakeVec3(part.X, part.Y, part.Z), part.RY) != null) { ok = true; break; }
                }
                sb.AppendLine($"  - {(ok ? "已生成" : "已记录")} {part.Prefab} @ ({part.X},{part.Y},{part.Z})");
                if (ok) spawned++;
            }
            LastPlaceLog = $"蓝图『{preset.Name}』：{spawned}/{preset.Parts.Count} 通过游戏API生成。\n" + sb;
            Core.Log.Info("[Blueprint] " + LastPlaceLog.Replace("\n", " "));
        }
    }

    // ------------------------------------------------------------------ Vehicle controls
    public static class VehicleControls
    {
        public static bool Boost, Clean, Cruise, Suck;
        public static float BoostMult = 2.2f;
        public static float CruiseSpeed = 6f;
        private static object _vehicle;

        public static void Init() { }

        public static void Update(float dt)
        {
            if (!Core.ModConfig.EnableVehicleControls.Value) return;
            if (_vehicle == null) _vehicle = ResolveVehicle();
            if (_vehicle == null) return;

            // Apply modifiers to the live vehicle through reflection.
            TrySet("boostActive", Boost);
            TrySet("boost", Boost);
            TrySet("boostMultiplier", Boost ? BoostMult : 1f);
            TrySet("cleaningActive", Clean);
            TrySet("vacuumActive", Clean);
            TrySet("suctionActive", Suck);
            TrySet("suckActive", Suck);
            TrySet("cruiseActive", Cruise);
            TrySet("cruiseSpeed", Cruise ? CruiseSpeed : 0f);
            if (Cruise) TryCall("SetCruise", CruiseSpeed);
        }

        static object ResolveVehicle()
        {
            foreach (var vt in GameNames.VehicleTypes)
            {
                var t = Helpers.FirstType(vt);
                if (t == null) continue;
                var inst = Helpers.FindSingleton(t);
                if (inst != null) { Core.Log.Info("[Vehicle] Resolved " + t.FullName); return inst; }
            }
            return null;
        }

        static void TrySet(string field, bool v) { Core.Reflect.SetField(_vehicle, field, v); }
        static void TrySet(string field, float v) { Core.Reflect.SetField(_vehicle, field, v); }
        static void TryCall(string method, float v) { Core.Reflect.Call(_vehicle, method, v); }

        public static void ToggleBoost() { Boost = !Boost; Core.Log.Info("[Vehicle] Boost=" + Boost); }
        public static void ToggleClean() { Clean = !Clean; }
        public static void ToggleCruise() { Cruise = !Cruise; }
        public static void ToggleSuck() { Suck = !Suck; }
    }

    // ------------------------------------------------------------------ Garage markers (Peak-style)
    public class GarageMarker
    {
        public string Name = "标记";
        public float X, Y, Z;
        public int ColorIndex;
        public object Visual; // optional live GameObject
    }

    public static class GarageMarkers
    {
        public static List<GarageMarker> Markers = new List<GarageMarker>();
        public static string FilePath { get; set; }
        public static int NextColor;

        public static void Init(string path) { FilePath = path; Load(); }

        public static void Save()
        {
            try
            {
                var sb = new StringBuilder();
                foreach (var m in Markers)
                    sb.AppendLine($"MARKER:{m.Name}|{m.ColorIndex}|{m.X}|{m.Y}|{m.Z}");
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath) ?? ".");
                File.WriteAllText(FilePath, sb.ToString());
            }
            catch (Exception e) { Core.Log.Error("[Markers] Save: " + e); }
        }

        public static void Load()
        {
            Markers.Clear();
            try
            {
                if (!File.Exists(FilePath)) return;
                foreach (var line in File.ReadAllLines(FilePath))
                {
                    if (!line.StartsWith("MARKER:")) continue;
                    var p = line.Substring(7).Split('|');
                    if (p.Length >= 5)
                        Markers.Add(new GarageMarker
                        {
                            Name = p[0],
                            ColorIndex = int.Parse(p[1]),
                            X = float.Parse(p[2], System.Globalization.CultureInfo.InvariantCulture),
                            Y = float.Parse(p[3], System.Globalization.CultureInfo.InvariantCulture),
                            Z = float.Parse(p[4], System.Globalization.CultureInfo.InvariantCulture)
                        });
                }
            }
            catch (Exception e) { Core.Log.Error("[Markers] Load: " + e); }
        }

        /// <summary>Add a marker at a world point. Spawns a visible primitive if the runtime allows.</summary>
        public static void AddMarker(Vector3 worldPos)
        {
            var m = new GarageMarker { Name = "标记#" + (Markers.Count + 1), X = worldPos.x, Y = worldPos.y, Z = worldPos.z, ColorIndex = NextColor++ % 4 };
            // try to create a small visible pin in the world
            try
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                if (go != null)
                {
                    Core.Reflect.SetPosition(go, worldPos.x, worldPos.y + 0.5f, worldPos.z);
                    m.Visual = go;
                }
            }
            catch (Exception e) { Core.Log.Error("[Markers] Visual: " + e); }
            Markers.Add(m);
            Save();
            Core.Log.Info($"[Markers] Added '{m.Name}' @ ({worldPos.x:F1},{worldPos.y:F1},{worldPos.z:F1})");
        }

        public static void ClearVisuals()
        {
            foreach (var m in Markers)
                if (m.Visual != null) { try { GameObject.Destroy((UnityEngine.Object)m.Visual); } catch { } m.Visual = null; }
        }
    }

    // ------------------------------------------------------------------ Auto-pilot (straight-line only)
    public class AutoPilotJob
    {
        public string Id;
        public string Vehicle = "";
        public string Kind = "clean";
        public Vector3 Start;
        public Vector3 End;
        public float Progress;       // 0..1 along the line
        public float TotalWork = 100;
        public float DoneWork;
        public bool Active = true;
        public bool Forward = true;
    }

    public static class AutoPilot
    {
        public static List<AutoPilotJob> Jobs = new List<AutoPilotJob>();
        public static string FilePath { get; set; }

        public static void Init(string path) { FilePath = path; Load(); }

        public static AutoPilotJob Assign(string vehicle, string kind, Vector3 start, Vector3 end, float totalWork)
        {
            var j = new AutoPilotJob
            {
                Id = Guid.NewGuid().ToString("N").Substring(0, 8),
                Vehicle = vehicle, Kind = kind, Start = start, End = end, TotalWork = totalWork, Active = true
            };
            Jobs.Add(j);
            Save();
            Core.Log.Info($"[AutoPilot] Assigned {kind} to {vehicle}: {j.TotalWork} work.");
            return j;
        }

        /// <summary>
        /// Move each active job along its straight line and accrue work. If the game's
        /// vehicle exposes a float "step forward" method we drive it for real; otherwise
        /// work is simulated (the gameplay rule "only straight lines" is always honoured).
        /// </summary>
        public static void Update(float dt)
        {
            if (!Core.ModConfig.EnableAutoPilot.Value) return;
            var speed = Core.ModConfig.AutoPilotSpeed.Value;
            var radius = Core.ModConfig.AutoPilotWorkRadius.Value;
            var vehicle = Helpers.FindSingleton(Helpers.FirstType(GameNames.VehicleTypes[0]));
            if (vehicle == null) vehicle = VehicleControlsTypeInstance();

            foreach (var j in Jobs)
            {
                if (!j.Active) continue;
                float lineLen = Vector3.Distance(j.Start, j.End);
                if (lineLen < 0.001f) continue;
                float step = (speed * dt) / lineLen;
                if (j.Forward) j.Progress += step; else j.Progress -= step;
                if (j.Progress >= 1f) { j.Progress = 1f; j.Forward = false; }
                if (j.Progress <= 0f) { j.Progress = 0f; j.Forward = true; }

                var pos = Vector3.Lerp(j.Start, j.End, j.Progress);
                j.DoneWork += (speed * dt) / Math.Max(1f, lineLen) * radius; // work scales with distance
                if (j.DoneWork >= j.TotalWork) { j.DoneWork = j.TotalWork; j.Active = false; }

                if (vehicle != null)
                {
                    // attempt real straight-line motion via a float step method
                    bool moved = false;
                    foreach (var m in GameNames.MoveStepMethods)
                        if (Core.Reflect.Call(vehicle, m, speed * dt) != null) { moved = true; break; }
                    if (!moved) Core.Reflect.SetPosition(vehicle, pos.x, pos.y, pos.z);
                }
            }
        }

        static object VehicleControlsTypeInstance() => null;

        public static void Save()
        {
            try
            {
                var sb = new StringBuilder();
                foreach (var j in Jobs)
                    sb.AppendLine($"JOB:{j.Id}|{j.Vehicle}|{j.Kind}|{j.Start.x}|{j.Start.y}|{j.Start.z}|{j.End.x}|{j.End.y}|{j.End.z}|{j.TotalWork}|{j.DoneWork}|{(j.Active ? 1 : 0)}");
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath) ?? ".");
                File.WriteAllText(FilePath, sb.ToString());
            }
            catch (Exception e) { Core.Log.Error("[AutoPilot] Save: " + e); }
        }

        public static void Load()
        {
            Jobs.Clear();
            try
            {
                if (!File.Exists(FilePath)) return;
                foreach (var line in File.ReadAllLines(FilePath))
                {
                    if (!line.StartsWith("JOB:")) continue;
                    var p = line.Substring(4).Split('|');
                    if (p.Length < 12) continue;
                    Jobs.Add(new AutoPilotJob
                    {
                        Id = p[0], Vehicle = p[1], Kind = p[2],
                        Start = new Vector3(F(p[3]), F(p[4]), F(p[5])),
                        End = new Vector3(F(p[6]), F(p[7]), F(p[8])),
                        TotalWork = F(p[9]), DoneWork = F(p[10]), Active = p[11] == "1"
                    });
                }
            }
            catch (Exception e) { Core.Log.Error("[AutoPilot] Load: " + e); }
        }

        static float F(string s) => float.Parse(s, System.Globalization.CultureInfo.InvariantCulture);
    }
}
