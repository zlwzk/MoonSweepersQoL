using System;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.IL2CPP;
using HarmonyLib;
using UnityEngine;

namespace MoonSweepersMod
{
    [BepInPlugin("com.dragdropgames.moonsweepers.qol", "Moon Sweepers QoL Mod", "1.0.0")]
    [BepInProcess("MoonSweepers.exe")]
    public class Plugin : BasePlugin
    {
        public override void Load()
        {
            Core.Log.Init(Log);
            try
            {
                Core.ModConfig.Init(Config);
                ResolvePaths();

                // --- read the player's real key bindings from Steam Input ---
                IO.KeybindingReader.Load(Core.Paths.SteamInputDir, Core.ModConfig.PreferredController.Value);
                IO.SaveInspector.Scan(UnityEngine.Application.persistentDataPath);
                Core.Log.Info("[MoonQoL] Keybindings loaded=" + IO.KeybindingReader.Loaded +
                              " | save regions=" + IO.SaveInspector.Regions);

                // --- feature systems (safe, self-contained) ---
                Features.OfflineProgress.Init(Path.Combine(Core.Paths.PluginDir, "offline_ledger.txt"));
                Features.BlueprintSystem.Init(Path.Combine(Core.Paths.PluginDir, "blueprints.txt"));
                Features.GarageMarkers.Init(Path.Combine(Core.Paths.PluginDir, "markers.txt"));
                Features.AutoPilot.Init(Path.Combine(Core.Paths.PluginDir, "autopilot.txt"));
                Features.VehicleControls.Init();

                // --- discover game internals for later hook finalisation ---
                string dump = Path.Combine(Core.Paths.PluginDir, "type_dump.txt");
                if (!File.Exists(dump)) Core.TypeDumper.Dump(dump);

                // --- in-game panel (IL2CPP MonoBehaviour via ClassInjector) ---
                RegisterPanel();

                var harmony = new Harmony("com.dragdropgames.moonsweepers.qol");
                Core.Log.Info("[MoonQoL] Loaded successfully. UI=" + Core.ModConfig.EnableUi.Value);
            }
            catch (Exception e)
            {
                Core.Log.Error("[MoonQoL] Load failed: " + e);
            }
        }

        static void ResolvePaths()
        {
            string loc = "";
            try { loc = Assembly.GetExecutingAssembly()?.Location ?? ""; } catch { }
            if (string.IsNullOrEmpty(loc))
                loc = Path.GetFullPath(Path.Combine(BepInEx.Paths.PluginPath, "MoonSweepersQoL"));
            Core.Paths.PluginDir = Path.GetDirectoryName(loc);
            Core.Paths.DataDir = Path.GetFullPath(Path.Combine(Core.Paths.PluginDir, "..", ".."));
            // SteamInput lives under the game's StreamingAssets
            try
            {
                string dataPath = UnityEngine.Application.dataPath ?? "";
                if (!string.IsNullOrEmpty(dataPath))
                    Core.Paths.SteamInputDir = Path.Combine(dataPath, "StreamingAssets", "SteamInput");
            }
            catch { }
            if (string.IsNullOrEmpty(Core.Paths.SteamInputDir))
                Core.Paths.SteamInputDir = Path.GetFullPath(Path.Combine(Core.Paths.DataDir, "..", "MoonSweepers_Data", "StreamingAssets", "SteamInput"));
        }

        static void RegisterPanel()
        {
            try
            {
                // Use the baseline Type-only overload: it exists in every
                // Il2CppInterop 1.x build, unlike the preview-only Options overload.
                Il2CppInterop.Runtime.Injection.ClassInjector.RegisterTypeInIl2Cpp(typeof(UI.ModPanel));
                var go = new GameObject("MoonQoL_Panel");
                go.AddComponent<UI.ModPanel>();
                Core.Log.Info("[MoonQoL] In-game panel registered.");
            }
            catch (Exception e)
            {
                Core.Log.Error("[MoonQoL] Panel registration failed (features still run, UI disabled): " + e);
            }
        }
    }
}
