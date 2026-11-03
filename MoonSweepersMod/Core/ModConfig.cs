using BepInEx.Configuration;

namespace MoonSweepersMod.Core
{
    /// <summary>
    /// Central configuration for the Moon Sweepers QoL mod. All entries are written
    /// to BepInEx's config file so the user (and friends) can tweak behaviour.
    /// </summary>
    public static class ModConfig
    {
        public static ConfigFile Config { get; private set; }

        // ---- master toggles ----
        public static ConfigEntry<bool> EnableBlueprints;
        public static ConfigEntry<bool> EnableVehicleControls;
        public static ConfigEntry<bool> EnableGarageMarkers;
        public static ConfigEntry<bool> EnableAutoPilot;
        public static ConfigEntry<bool> EnableOfflineProgress;
        public static ConfigEntry<bool> EnableUi;

        // ---- UI ----
        public static ConfigEntry<UnityEngine.KeyCode> UiToggleKey;
        public static ConfigEntry<UnityEngine.KeyCode> MarkerPlaceKey;

        // ---- key-binding integration ----
        public static ConfigEntry<bool> ReadGameBindings;
        public static ConfigEntry<string> PreferredController;

        // ---- vehicle quick actions: which in-game action drives each command ----
        public static ConfigEntry<string> VehicleBoostAction;
        public static ConfigEntry<string> VehicleCleanAction;
        public static ConfigEntry<string> VehicleCruiseAction;
        public static ConfigEntry<string> VehicleSuckAction;

        // ---- garage markers ----
        public static ConfigEntry<string> MarkerAction;

        // ---- autopilot ----
        public static ConfigEntry<float> AutoPilotSpeed;
        public static ConfigEntry<bool> AutoPilotStraightOnly;
        public static ConfigEntry<float> AutoPilotWorkRadius;

        // ---- offline progress ----
        public static ConfigEntry<float> OfflineWorkPerHour;
        public static ConfigEntry<string> OfflineLedgerPath;

        public static void Init(ConfigFile cfg)
        {
            Config = cfg;

            const string main = "MoonQoL.General";
            EnableBlueprints = cfg.Bind(main, "EnableBlueprints", true, "快速蓝图：预设搭建与自定义预设保存");
            EnableVehicleControls = cfg.Bind(main, "EnableVehicleControls", true, "车辆快捷操作：加速/清洁/定速巡航/吸力");
            EnableGarageMarkers = cfg.Bind(main, "EnableGarageMarkers", true, "地库标记：点击快速标记位置 (参考 Peak)");
            EnableAutoPilot = cfg.Bind(main, "EnableAutoPilot", true, "人机自动驾驶：分配任务，车辆走直线");
            EnableOfflineProgress = cfg.Bind(main, "EnableOfflineProgress", true, "离线进度：关游戏也能继续工作");
            EnableUi = cfg.Bind(main, "EnableUi", true, "显示原生风格控制面板");

            const string ui = "MoonQoL.UI";
            UiToggleKey = cfg.Bind(ui, "TogglePanelKey", UnityEngine.KeyCode.F8, "开关控制面板");
            MarkerPlaceKey = cfg.Bind(ui, "PlaceMarkerKey", UnityEngine.KeyCode.M, "放置地库标记");

            const string keys = "MoonQoL.Keybindings";
            ReadGameBindings = cfg.Bind(keys, "ReadGameBindings", true, "主动读取游戏的 Steam Input 按键绑定");
            PreferredController = cfg.Bind(keys, "PreferredController", "controller_xboxone", "优先读取的手柄/输入配置 VDF (留空=自动)");
            VehicleBoostAction = cfg.Bind(keys, "VehicleBoostAction", "jump", "映射到『加速』的游戏动作 (jump=Boost)");
            VehicleCleanAction = cfg.Bind(keys, "VehicleCleanAction", "use_object", "映射到『清洁』的游戏动作 (use_object=Drive)");
            VehicleCruiseAction = cfg.Bind(keys, "VehicleCruiseAction", "secondary", "映射到『定速巡航』的游戏动作 (secondary=Brake)");
            VehicleSuckAction = cfg.Bind(keys, "VehicleSuckAction", "use_tool", "映射到『吸力』的游戏动作 (use_tool=Place)");
            MarkerAction = cfg.Bind(keys, "MarkerAction", "interact", "映射到『放置标记』的游戏动作 (interact=Build menu)");

            const string ap = "MoonQoL.AutoPilot";
            AutoPilotSpeed = cfg.Bind(ap, "Speed", 6f, "自动驾驶直线速度 (m/s)");
            AutoPilotStraightOnly = cfg.Bind(ap, "StraightOnly", true, "车辆只能走直线 (符合玩法设定)");
            AutoPilotWorkRadius = cfg.Bind(ap, "WorkRadius", 8f, "每台车/机器人作业半径 (米)");

            const string off = "MoonQoL.Offline";
            OfflineWorkPerHour = cfg.Bind(off, "WorkPerHour", 120f, "离线时每小时完成的工作量 (工作单元/小时)");
            OfflineLedgerPath = cfg.Bind(off, "LedgerPath", "", "离线账本文件路径 (留空=插件目录)");

            Log.Info("[MoonQoL] Configuration initialised.");
        }
    }
}
