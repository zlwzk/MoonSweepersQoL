# Moon Sweepers · QoL Mod（玩法增强）

为 **Moon Sweepers**（Unity 6 / IL2CPP）制作的 BepInEx 6 Mod，新增 6 大玩法增强功能。

## 功能一览
1. **蓝图系统** —— 快速放置预设结构；预设支持用户自行保存 / 添加自定义预设。
2. **车辆快捷操作** —— 一键加速 / 清洁 / 定速巡航 / 吸力；**自动读取你的 Steam Input 按键绑定**。
3. **地库 / 车位标记** —— 点击地面快速标记，参考 Peak 的标记逻辑。
4. **人机自动驾驶** —— 无需真人坐车即可启动；给每辆车分配任务；**车辆只能走直线**（参考扫地机器人）。
5. **统一画风** —— 面板 / 按钮采用贴近原生的深色半透明高对比 HUD 风格。
6. **离线不丢进度** —— 关闭游戏后已分配的车辆 / 机器人任务按关闭时长结算，重开游戏时入账；不修改游戏原本存档。

## 给玩家（发给朋友）
直接下载仓库 **Releases** 中的 `MoonSweepersQoL-vX.Y.Z.zip`，解压覆盖到游戏根目录
（`MoonSweepers.exe` 所在文件夹）即可。更省事可双击 `dist/install.bat` 自动安装。
进入游戏按 **F8** 打开功能面板。详见压缩包内 `README.md`。

## 开发者（构建源码）
```bash
# 需要 .NET 8+ SDK
dotnet build MoonSweepersMod/MoonSweepersMod.csproj -c Release
# 产物： MoonSweepersMod/bin/Release/net8.0/MoonSweepersMod.dll
```
- `refs/`：BepInEx / Il2CppInterop 等参考程序集（用于编译）。
- `stubs/`：因官方 `Il2CppInterop.Runtime` / `BepInEx.Unity.IL2CPP` 的托管元数据无法被编译器解析，
  自建同名同版本桩程序集，运行时由真实 BepInEx 提供实现。
- `UnityEngineStub/`：编译期用的 UnityEngine 桩。
- `MoonSweepersMod/`：Mod 主工程（运行时通过 `Il2CppSystem.Reflection` 反射对接游戏内部，全部防御性降级）。

## 技术说明
由于 IL2CPP 把代码编译为原生 C++，且官方 DLL 元数据无法离线解析，本 Mod 采用
**运行时反射 + 优雅降级** 的方式对接游戏类型：即使某些内部方法名对不上，也只会记日志、不会让游戏崩溃。
首次运行会在插件目录生成 `type_dump.txt`（全部 IL2CPP 类型），可用于后续精确对接游戏 API。

License：本 Mod 以 MIT 许可证发布；BepInEx 与其自身许可证一同分发。
