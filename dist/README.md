# Moon Sweepers · QoL 功能增强 Mod

为 **Moon Sweepers**（Unity 6 / IL2CPP）制作的玩法增强 Mod，基于 BepInEx 6 加载。
本包已自带 BepInEx 运行环境，解压即用，方便直接发给朋友游玩。

---

## 一、安装方法（二选一）

### 方法 A：解压覆盖（最简单）
把本压缩包内的 **全部内容**（`BepInEx` 文件夹、`winhttp.dll`、`doorstop_config.ini`、`dotnet` 文件夹）
直接解压 / 覆盖到游戏根目录，也就是 `MoonSweepers.exe` 所在的文件夹：

```
D:\Program Files (x86)\Steam\steamapps\common\Moon Sweepers
```

### 方法 B：用安装器（自动定位）
双击运行 `install.bat`，它会自动从 Steam 注册表找到游戏目录并复制文件。
如果自动定位失败，会请你手动输入游戏根目录路径。

> 安装完成后，游戏根目录应出现 `BepInEx`、`winhttp.dll` 等文件。
> 启动时游戏会加载 BepInEx，首次运行会在 `BepInEx\plugins\MoonSweepersQoL\` 下生成配置与数据文件。

---

## 二、卸载
- 删除游戏根目录下的 `winhttp.dll` 与 `doorstop_config.ini`；
- 删除 `BepInEx\plugins\MoonSweepersQoL` 文件夹（以及不需要的 `BepInEx` 整体）。
游戏即恢复为原版。

---

## 三、游戏内操作

进入游戏后，按 **F8** 打开 / 关闭功能面板（右上角）。
面板共有 6 个标签页：总览 / 蓝图 / 车辆 / 标记 / 自动驾驶 / 离线。

### 1. 蓝图（Blueprint）
- 快速放置预设好的蓝图结构。
- **预设支持自定义**：在“蓝图”页可保存当前布置为预设、加载预设、并添加你自己命名的预设。
- 预设与数据保存在 `BepInEx\plugins\MoonSweepersQoL\blueprints.txt`。

### 2. 车辆快捷操作（Vehicle QoL）
- 为车辆提供一键 **加速 / 清洁 / 定速巡航 / 吸力** 等快捷操作。
- **自动读取你的游戏按键绑定**：Mod 会读取 Steam Input 的动作清单（`StreamingAssets\SteamInput` 下的 VDF 文件），
  并在面板里显示你当前真实的按键 / 手柄映射，操作完全贴合你的习惯。
- 各项开关与按键可在 `BepInEx\config\com.dragdropgames.moonsweepers.qol.cfg` 中自行配置。

### 3. 地库 / 车位标记（Marker）
- 参考 Peak 的标记逻辑：点击即可在地面上快速标记一个位置。
- 在“标记”页进入放置模式后，用鼠标点击地面（射线检测）即可落点，并可用 M 键快速放置。
- 标记以可见的圆柱显示，数据保存在 `markers.txt`，方便你记录清洁路线、车位、重点区域。

### 4. 人机自动驾驶（Auto-Pilot，仅直线）
- 无需真人玩家坐在车上即可启动：给每辆车分配一个任务（清洁 / 运输等）。
- 玩法参考游戏内的扫地机器人，但**车辆只能走直线**（沿起点→终点的直线插值前进），符合“直线工作”的规则。
- 可在“自动驾驶”页为选中车辆分配起点 / 终点 / 任务类型 / 工作量。

### 5. 统一画风（UI）
- 新增面板与按钮整体采用深色半透明、圆角、高对比的原生 HUD 风格，尽量贴近游戏原本的按键 / 按钮观感。

### 6. 离线不丢进度（Offline Work）
- “关闭游戏不影响工作”：已分配的车辆 / 机器人任务进度不会因为退出游戏而丢失。
- 重新打开游戏时，Mod 会按你**关闭的时长**结算离线工作量（按墙钟时间计算），
  并以报告形式在“离线”页展示这段时间的产出。
- 离线账本保存在 `offline_ledger.txt`，**绝不修改游戏原本的存档（.moon）**，安全可逆。

---

## 四、配置文件
首次运行后生成：`BepInEx\config\com.dragdropgames.moonsweepers.qol.cfg`
可调整：各功能总开关、面板热键（默认 F8）、标记放置键（默认 M）、
偏好手柄（默认 Xbox）、各项车辆动作映射、自动驾驶速度、离线每小时工作量等。

---

## 五、故障排查
- 若面板没出现：检查游戏根目录是否存在 `winhttp.dll` 与 `BepInEx` 文件夹；
  查看 `BepInEx\LogOutput.log` 中是否包含 `[MoonQoL]` 相关日志。
- 本 Mod 采用“尽力而为”的运行时反射对接游戏内部接口：即便某些游戏内部方法名对不上，
  也只会记日志、不会让游戏崩溃。若某功能未完全生效，可把 `type_dump.txt` 反馈给作者进一步微调。
- 本 Mod 不修改任何游戏存档，可随时卸载。

---

## 六、技术说明（给进阶用户）
- 框架：BepInEx 6.0.0-pre.2（Unity IL2CPP，win-x64）+ Il2CppInterop + HarmonyX。
- 由于 IL2CPP 编译为原生代码、且官方 DLL 元数据无法离线解析，
  本 Mod 通过**运行时反射（Il2CppSystem.Reflection）** 对接游戏类型，所有调用均做了防御性降级。
- 首次运行会在插件目录生成 `type_dump.txt`（全部 IL2CPP 类型清单），用于后续精确对接游戏 API。

祝你清扫愉快！🧹
