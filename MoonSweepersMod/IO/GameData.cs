using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MoonSweepersMod.Core;

namespace MoonSweepersMod.IO
{
    /// <summary>
    /// Reads the game's Steam Input configuration so the mod can *actively* use the
    /// player's real button layout instead of invented defaults.
    /// </summary>
    public static class KeybindingReader
    {
        // action name -> human readable label (from the manifest localization)
        public static Dictionary<string, string> ActionLabels = new Dictionary<string, string>();
        // action name -> physical inputs (e.g. button_a, left_bumper)
        public static Dictionary<string, List<string>> ActionInputs = new Dictionary<string, List<string>>();
        public static string ActiveController = "";
        public static bool Loaded { get; private set; }

        static readonly Dictionary<string, string> InputHuman = new Dictionary<string, string>
        {
            { "button_a", "A" }, { "button_b", "B" }, { "button_x", "X" }, { "button_y", "Y" },
            { "button_menu", "Menu" }, { "button_escape", "Esc" }, { "button_escape_hold", "Esc(hold)" },
            { "left_bumper", "LB" }, { "right_bumper", "RB" }, { "left_trigger", "LT" }, { "right_trigger", "RT" },
            { "dpad_north", "D-Up" }, { "dpad_south", "D-Down" }, { "dpad_east", "D-Right" }, { "dpad_west", "D-Left" },
            { "stick_pad_gyro", "LStick" }, { "joystick", "LStick" }, { "right_joystick", "RStick" },
            { "left_trigger_full", "LT" }, { "right_trigger_full", "RT" }
        };

        public static string Humanize(string input)
        {
            if (input == null) return "?";
            return InputHuman.TryGetValue(input, out var h) ? h : input;
        }

        /// <summary>
        /// Load bindings. <paramref name="steamInputDir"/> is usually
        /// Application.dataPath + "/StreamingAssets/SteamInput".
        /// </summary>
        public static void Load(string steamInputDir, string preferredController)
        {
            try
            {
                if (string.IsNullOrEmpty(steamInputDir) || !Directory.Exists(steamInputDir))
                {
                    Log.Warn("[Keys] SteamInput dir not found: " + steamInputDir);
                    Loaded = false; return;
                }

                var manifestPath = Path.Combine(steamInputDir, "moon_cleaners_actions.vdf");
                if (!File.Exists(manifestPath)) { Log.Warn("[Keys] manifest missing"); Loaded = false; return; }
                var manifest = VdfParser.Parse(File.ReadAllText(manifestPath));

                // Collect action names + localized labels from the "Play" set.
                var play = VdfParser.Get(manifest, "actions", "Play");
                if (play != null)
                {
                    CollectActions(play);
                }
                var eng = VdfParser.Get(manifest, "localization", "english");
                if (eng != null)
                {
                    foreach (var kv in eng.Children)
                        if (kv.Value.IsValue) ActionLabels[kv.Key] = kv.Value.Value;
                }

                // Choose controller file.
                string ctrlFile = string.IsNullOrEmpty(preferredController) ? "controller_xboxone.vdf" : preferredController;
                if (!File.Exists(Path.Combine(steamInputDir, ctrlFile)))
                {
                    // fall back to any available controller
                    foreach (var f in Directory.GetFiles(steamInputDir, "controller_*.vdf"))
                    { ctrlFile = Path.GetFileName(f); break; }
                }
                ActiveController = ctrlFile;
                var ctrl = VdfParser.Parse(File.ReadAllText(Path.Combine(steamInputDir, ctrlFile)));

                // Map group modes to actions (gameactions), and buttons to actions (bindings).
                foreach (var g in ctrl.Children)
                {
                    if (g.Key != "group") continue;
                    var node = g.Value;
                    // direct button bindings
                    var inputs = VdfParser.Get(node, "inputs");
                    if (inputs != null)
                    {
                        foreach (var inp in inputs.Children)
                        {
                            var binding = VdfParser.Get(inp.Value, "activators", "Full_Press", "bindings", "binding");
                            if (binding != null && binding.IsValue)
                                Link(binding.Value, inp.Key);
                            var binding2 = VdfParser.Get(inp.Value, "activators", "Release_Press", "bindings", "binding");
                            if (binding2 != null && binding2.IsValue)
                                Link(binding2.Value, inp.Key);
                        }
                    }
                    // groups bound to an action (move/look/use_tool/secondary)
                    var ga = node["gameactions"];
                    if (ga != null)
                    {
                        foreach (var a in ga.Children)
                            if (a.Value.IsValue) LinkActionToGroup(a.Value.Value, g.Key);
                    }
                }

                Loaded = true;
                Log.Info("[Keys] Loaded " + ActionInputs.Count + " action bindings from " + ctrlFile);
            }
            catch (Exception e) { Log.Error("[Keys] " + e); Loaded = false; }
        }

        static void CollectActions(VdfNode play)
        {
            // Button actions
            var btn = play["Button"];
            if (btn != null) foreach (var a in btn.Children) RegisterAction(a.Key);
            // AnalogTrigger
            var at = play["AnalogTrigger"];
            if (at != null) foreach (var a in at.Children) RegisterAction(a.Key);
            // StickPadGyro (virtual axes)
            var sp = play["StickPadGyro"];
            if (sp != null) foreach (var a in sp.Children) RegisterAction(a.Key);
        }

        static void RegisterAction(string action)
        {
            if (!ActionInputs.ContainsKey(action)) ActionInputs[action] = new List<string>();
        }

        static void Link(string binding, string input)
        {
            // binding format: "game_action Play <action>"
            if (binding == null) return;
            var parts = binding.Split(' ');
            if (parts.Length >= 4 && parts[0] == "game_action" && parts[1] == "Play")
            {
                var action = parts[2];
                RegisterAction(action);
                if (!ActionInputs[action].Contains(input)) ActionInputs[action].Add(input);
            }
        }

        static readonly Dictionary<string, List<string>> GroupToAction = new Dictionary<string, List<string>>();
        static void LinkActionToGroup(string action, string groupId)
        {
            RegisterAction(action);
            if (!ActionInputs.ContainsKey(action)) ActionInputs[action] = new List<string>();
            var label = "group:" + groupId;
            if (!ActionInputs[action].Contains(label)) ActionInputs[action].Add(label);
        }

        public static string DescribeAction(string action)
        {
            if (action == null) return "?";
            if (!ActionInputs.TryGetValue(action, out var list) || list.Count == 0) return "(未绑定)";
            var sb = new StringBuilder();
            foreach (var inp in list) sb.Append(Humanize(inp)).Append(" ");
            return sb.ToString().Trim();
        }
    }

    /// <summary>
    /// Lightweight inspection of the game's save folder. We never modify the
    /// proprietary .moon files; we only read the public slot.json for display and
    /// keep our own offline ledger for progress that continues while the game is off.
    /// </summary>
    public static class SaveInspector
    {
        public static string SaveRoot { get; set; } = "";
        public static double LastPlayedTicks { get; set; }
        public static double Dollars { get; set; }
        public static double Regions { get; set; }
        public static double PlaySeconds { get; set; }

        public static void Scan(string persistentDataPath)
        {
            try
            {
                // persistentDataPath = .../LocalLow/DragDropGames/MoonSweepers
                var root = persistentDataPath;
                if (string.IsNullOrEmpty(root) || !Directory.Exists(root)) root = SaveRoot;
                if (string.IsNullOrEmpty(root) || !Directory.Exists(root)) return;

                var sessions = Path.Combine(root, "MoonSessions");
                if (!Directory.Exists(sessions)) return;
                var slots = Directory.GetDirectories(sessions);
                string slotDir = slots.Length > 0 ? slots[0] : null;
                if (slotDir == null) return;

                var slotJson = Path.Combine(slotDir, "slot.json");
                if (File.Exists(slotJson)) ParseSlotJson(File.ReadAllText(slotJson));
            }
            catch (Exception e) { Log.Error("[Save] Scan: " + e); }
        }

        static void ParseSlotJson(string json)
        {
            LastPlayedTicks = JsonReadDouble(json, "lastPlayedTicks");
            Dollars = JsonReadDouble(json, "dollars");
            Regions = JsonReadDouble(json, "regions");
            PlaySeconds = JsonReadDouble(json, "playSeconds");
        }

        public static double JsonReadDouble(string json, string key)
        {
            try
            {
                var idx = json.IndexOf("\"" + key + "\"", StringComparison.Ordinal);
                if (idx < 0) return 0;
                var colon = json.IndexOf(':', idx);
                if (colon < 0) return 0;
                int s = colon + 1;
                while (s < json.Length && (json[s] == ' ' || json[s] == '"')) s++;
                int e = s;
                while (e < json.Length && "0123456789.-eE".IndexOf(json[e]) >= 0) e++;
                var num = json.Substring(s, e - s);
                return double.TryParse(num, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var v) ? v : 0;
            }
            catch { return 0; }
        }
    }
}
