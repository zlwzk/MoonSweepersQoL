using UnityEngine;
using BepInEx.Configuration;

namespace MoonSweepersMod.UI
{
    /// <summary>
    /// In-game control panel drawn with Unity IMGUI and styled to echo the game's
    /// clean, rounded, high-contrast HUD look. Registered as an IL2CPP MonoBehaviour
    /// via ClassInjector so Unity calls Awake/Update/OnGUI on it.
    /// </summary>
    public class ModPanel : MonoBehaviour
    {
        public bool Visible = true;
        private bool _placementMode;
        private int _tab;

        // NOTE: intentionally `new`, not `override`. Il2CppInterop's generated
        // UnityEngine.MonoBehaviour does NOT declare Unity message methods (Awake/
        // Update/OnGUI) as virtual; overriding them would cause a TypeLoadException
        // when the injected class is loaded. Unity's message system finds these by
        // name, so a plain new-slot method works with either base-class shape.
        public new void Awake() { Visible = Core.ModConfig.EnableUi.Value; }

        public new void Update()
        {
            // feature ticks run every frame regardless of panel visibility
            Features.VehicleControls.Update(Time.deltaTime);
            Features.AutoPilot.Update(Time.deltaTime);
            Features.OfflineProgress.Tick(Time.deltaTime);

            if (UnityEngine.Input.GetKeyDown(Core.ModConfig.UiToggleKey.Value)) Visible = !Visible;
        }

        public new void OnGUI()
        {
            if (!Visible) return;
            int w = Screen.width, h = Screen.height;
            Rect panel = new Rect(w - 360, 20, 340, h - 40);

            GUI.color = new Color(0.07f, 0.09f, 0.13f, 0.92f);
            GUI.Box(panel, "");
            GUI.color = new Color(1, 1, 1, 1);

            GUI.BeginGroup(panel);
            GUI.Label(new Rect(10, 8, 320, 22), "Moon Sweepers · QoL Mod");
            GUI.Label(new Rect(10, 30, 320, 16), "控制器: " + (IO.KeybindingReader.ActiveController ?? "?"));

            string[] tabs = { "总览", "蓝图", "车辆", "标记", "自动驾驶", "离线" };
            for (int i = 0; i < tabs.Length; i++)
                if (GUI.Button(new Rect(10 + i * 52, 48, 50, 20), tabs[i])) _tab = i;

            int top = 76;
            if (_tab == 0) DrawOverview(top);
            else if (_tab == 1) DrawBlueprints(top);
            else if (_tab == 2) DrawVehicle(top);
            else if (_tab == 3) DrawMarkers(top);
            else if (_tab == 4) DrawAutoPilot(top);
            else if (_tab == 5) DrawOffline(top);

            GUI.EndGroup();
        }

        void DrawOverview(int top)
        {
            Toggle(new Rect(10, top, 320, 20), "蓝图功能", Core.ModConfig.EnableBlueprints);
            Toggle(new Rect(10, top + 24, 320, 20), "车辆快捷操作", Core.ModConfig.EnableVehicleControls);
            Toggle(new Rect(10, top + 48, 320, 20), "地库标记", Core.ModConfig.EnableGarageMarkers);
            Toggle(new Rect(10, top + 72, 320, 20), "人机自动驾驶", Core.ModConfig.EnableAutoPilot);
            Toggle(new Rect(10, top + 96, 320, 20), "离线进度", Core.ModConfig.EnableOfflineProgress);

            GUI.Label(new Rect(10, top + 130, 320, 20), "存档: 区域 " + IO.SaveInspector.Regions + " · $" + IO.SaveInspector.Dollars);
            if (GUI.Button(new Rect(10, top + 156, 320, 22), "导出游戏类型 (type_dump.txt)"))
                Core.TypeDumper.Dump(System.IO.Path.Combine(Core.Paths.PluginDir, "type_dump.txt"));
            GUI.Label(new Rect(10, top + 184, 320, 50),
                "提示: 导出的类型表用于精确定位游戏内部API，\n以便把玩法钩子从『尽力而为』升级为『完全生效』。");
        }

        void DrawBlueprints(int top)
        {
            for (int i = 0; i < Features.BlueprintSystem.Presets.Count; i++)
            {
                var p = Features.BlueprintSystem.Presets[i];
                int y = top + i * 26;
                GUI.Label(new Rect(10, y, 200, 22), p.Name + " (" + p.Parts.Count + " 件)");
                if (GUI.Button(new Rect(214, y, 50, 22), "放置")) Features.BlueprintSystem.Place(p);
                if (GUI.Button(new Rect(268, y, 50, 22), "删除")) Features.BlueprintSystem.RemovePreset(i);
            }
            int baseY = top + Features.BlueprintSystem.Presets.Count * 26 + 6;
            if (GUI.Button(new Rect(10, baseY, 320, 22), "保存当前为预设 (尽力读取建造选择)"))
                Features.BlueprintSystem.AddPreset(new Features.BlueprintPreset { Name = "自定义 " + System.DateTime.Now.ToString("HH:mm") });
            GUI.Label(new Rect(10, baseY + 28, 320, 60), Features.BlueprintSystem.LastPlaceLog);
        }

        void DrawVehicle(int top)
        {
            GUI.Label(new Rect(10, top, 320, 20), "车辆指令（已读取你的按键绑定）:");
            DrawCmd(top + 24, "加速 Boost", Core.ModConfig.VehicleBoostAction, Features.VehicleControls.Boost, Features.VehicleControls.ToggleBoost);
            DrawCmd(top + 48, "清洁 Clean", Core.ModConfig.VehicleCleanAction, Features.VehicleControls.Clean, Features.VehicleControls.ToggleClean);
            DrawCmd(top + 72, "定速巡航 Cruise", Core.ModConfig.VehicleCruiseAction, Features.VehicleControls.Cruise, Features.VehicleControls.ToggleCruise);
            DrawCmd(top + 96, "吸力 Suck", Core.ModConfig.VehicleSuckAction, Features.VehicleControls.Suck, Features.VehicleControls.ToggleSuck);
            GUI.Label(new Rect(10, top + 130, 320, 40), "点击按钮切换；真实生效需游戏内类型导出。\n(你的游戏按键: 见各指令后的『键:』)");
        }

        void DrawCmd(int y, string label, ConfigEntry<string> actionKey, bool state, System.Action toggle)
        {
            GUI.Label(new Rect(10, y, 150, 20), label);
            GUI.Label(new Rect(160, y, 90, 20), "键: " + IO.KeybindingReader.DescribeAction(actionKey.Value));
            if (GUI.Button(new Rect(252, y, 40, 20), state ? "开" : "关")) toggle();
        }

        void DrawMarkers(int top)
        {
            if (GUI.Button(new Rect(10, top, 320, 22), _placementMode ? "点击地面放置(再按M取消)" : "进入放置模式 (M)"))
                _placementMode = !_placementMode;
            GUI.Label(new Rect(10, top + 28, 320, 16), "参考 Peak：点击即标记位置");

            if (_placementMode && Event.current != null && Event.current.isMouse && Event.current.button == 0 && Camera.main != null)
            {
                var mp = Event.current.mousePosition;
                var ray = Camera.main.ScreenPointToRay(new Vector3(mp.x, mp.y, 0));
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    var p = hit.point;
                    Features.GarageMarkers.AddMarker(new Vector3(p.x, p.y, p.z));
                    _placementMode = false;
                }
            }

            for (int i = 0; i < Features.GarageMarkers.Markers.Count; i++)
            {
                var m = Features.GarageMarkers.Markers[i];
                GUI.Label(new Rect(10, top + 50 + i * 18, 320, 16),
                    m.Name + " @ (" + m.X.ToString("F1") + "," + m.Z.ToString("F1") + ")");
            }
        }

        void DrawAutoPilot(int top)
        {
            GUI.Label(new Rect(10, top, 320, 20), "直线自动驾驶（车辆只能走直线）");
            GUI.Label(new Rect(10, top + 22, 320, 16), "速度: " + Core.ModConfig.AutoPilotSpeed.Value + " m/s");
            if (GUI.Button(new Rect(10, top + 42, 320, 22), "分配扫地机器人: A→B 直线清扫"))
            {
                Features.AutoPilot.Assign("robot", "clean", new Vector3(0, 0, 0), new Vector3(20, 0, 0), 100);
                Features.OfflineProgress.AddTask("clean", "robot", 100);
            }
            for (int i = 0; i < Features.AutoPilot.Jobs.Count; i++)
            {
                var j = Features.AutoPilot.Jobs[i];
                GUI.Label(new Rect(10, top + 70 + i * 18, 320, 16),
                    j.Kind + " " + j.DoneWork.ToString("F0") + "/" + j.TotalWork.ToString("F0") + (j.Active ? "" : " 完成"));
            }
        }

        void DrawOffline(int top)
        {
            string report = Features.OfflineProgress.ReportString();
            GUI.Label(new Rect(10, top, 320, 80),
                report.Length > 0 ? report : "暂无离线进度报告。\n关闭游戏一段时间后再回来，\n分配的任务会按离线时长结算。");
            GUI.Label(new Rect(10, top + 90, 320, 16), "每小时工作量: " + Core.ModConfig.OfflineWorkPerHour.Value);
            if (GUI.Button(new Rect(10, top + 110, 320, 22), "清除报告")) Features.OfflineProgress.ClearReport();
        }

        void Toggle(Rect r, string label, ConfigEntry<bool> entry)
        {
            entry.Value = GUI.Toggle(r, entry.Value, label);
        }
    }
}
