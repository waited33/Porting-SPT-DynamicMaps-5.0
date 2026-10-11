# DynamicMaps 5.0BE — 重编译指南

SPT 5.0 / EFT 客户端 1.2.0.0.47965 的 DynamicMaps 移植版。本文件说明**游戏更新后如何重新编译**。

## 目录

```
DynamicMaps-5.0BE/
├─ Plugin/                    客户端 BepInEx 插件
│  ├─ DynamicMaps.csproj      构建配置（含自动部署 target）
│  ├─ Plugin.cs               插件入口 + 版本检查
│  ├─ Patches/                Harmony 补丁（挂载点）
│  ├─ UI/                     地图界面（ModdedMapScreen、MapPeekComponent 等）
│  ├─ DynamicMarkers/         各类标记提供者
│  ├─ Resources/              Maps/（地图定义 + 图层）、Markers/（图标）
│  ├─ SPTushonkaShim/         对 SPTushonka 反射 API 的本地垫片（见该目录 README.md）
│  └─ Utils/
├─ Server/                    服务端插件（SPT user/mods）
│  ├─ DynamicMapsServer.csproj
│  ├─ DynamicMapsServer.cs    IModMetadata + IOnLoad：6 张地图道具 + 商人 assort
│  ├─ CustomStaticRouter.cs   GET /dynamicmaps/load 下发标记可见性配置
│  ├─ config.json             标记可见性开关
│  └─ LICENSE
├─ tools/TypeDump/            离线类型检查工具（不用开游戏就能查程序集）
├─ tools/make-release.sh      一键出发布包
├─ tools/RELEASE-README.md    发布包内随附的安装说明
├─ releases/                  发布产物（已 gitignore，由 make-release.sh 生成）
├─ .gitignore
└─ .gitattributes
```

## 构建

前置：.NET 10 SDK；目标客户端已装好 BepInEx（即 `BepInEx/core`、`BepInEx/interop` 已生成）。

```bash
cd Plugin
dotnet build DynamicMaps.csproj -c Release
```

构建成功后 csproj 里的 `Deploy` target 会**自动**把产物复制到
`$(GameDir)/BepInEx/plugins/SPT-DynamicMaps/`（`DynamicMaps.dll`、`SPTushonka*.dll`、`Resources/**`）。

> 游戏正在运行时 DLL 会被占用，部署会失败（错误 MSB3027/MSB3021）。先退出游戏再构建。

构建会有 **2 个 CS0436 警告**（`Il2CppInjection` / `ClientAppUtils` 与已装模块的同名类型冲突）。
这是**预期的**，不是错误——原因为何、能不能删，见 `Plugin/SPTushonkaShim/README.md`。
只要不是 0 错误就没问题。

## 游戏更新后要改什么

1. **`Plugin/DynamicMaps.csproj`** — `<GameDir>` 指向新的客户端目录：
   ```xml
   <GameDir Condition="'$(GameDir)' == ''">D:\SPT\Client.<新版本号></GameDir>
   ```
   也可以不改文件，构建时传参覆盖：`dotnet build -c Release -p:GameDir="D:\SPT\Client.xxx"`。

2. **`Plugin/Plugin.cs`** — `TarkovVersion` 常量改成新客户端 build 号：
   ```csharp
   public const int TarkovVersion = 47965;   // ← EscapeFromTarkov.exe 的 FilePrivatePart
   ```
   取法：右键 `EscapeFromTarkov.exe` → 属性 → 详细信息 → 文件版本第四段（`1.2.0.0.47965` 里就是 `47965`）。
   对不上插件会记 `Invalid EFT version` 并直接 return（不加载）。

3. **先启动一次新客户端**，让 BepInEx 重新生成 `BepInEx/interop/*.dll`（游戏程序集的托管封装）。csproj 与补丁都依赖它。

4. **若游戏类型改名/移动了**，补丁挂载点要跟着改。本移植版踩过的坑：

   - EFT 的屏幕类常是"空派生类 + 泛型基类"结构。`AccessTools.Method(typeof(派生类), ...)` 拿到继承来的方法会被 Harmony 拒绝
     （`You can only patch implemented methods/constructors ... patch the declared method <基类>::... instead`）；
     改成 patch 开放泛型基类又会**静默不触发**。
     正确做法：沿 `BaseType` 往上找**第一个非泛型**类型，patch 它声明的方法。
     例：战斗 HUD 挂 `EFT.UI.Screens.UIScreen.ShowGameObject(bool)`，postfix 里 `__instance.TryCast<EftBattleUIScreen>()` 过滤。
   - 查类型结构用 `tools/TypeDump`（离线读程序集元数据，不用开游戏）。第一个参数是要读的文件夹
     （可用 `;` 分隔多个；它会自动带上 `BepInEx/core`、`BepInEx/interop` 供解析）：
     ```bash
     cd tools/TypeDump
     dotnet run -c Release -- "D:\SPT\Client.xxx\BepInEx\interop" "EFT.UI.EftBattleUIScreen"
     dotnet run -c Release -- "D:\SPT\Client.xxx\BepInEx\interop" "find:BattleUI"
     # 查已装模块的 API（对比 SPTushonkaShim 用）
     dotnet run -c Release -- "D:\SPT\Client.xxx\BepInEx\plugins\sptushonka" \
       "SPTushonka.Reflection.Il2Cpp.Il2CppInjection"
     ```

5. **`BepInDependency` GUID 必须与已装模块一致**（`Plugin/Plugin.cs` 顶部）：
   `sptushonka` 模块的 GUID 是 `com.sptushonka.*`，`SPTushonka Core` 为 `com.sptushonka.core`。
   GUID 写错不会报编译错误，只会让 **BepInEx 判定依赖缺失、整个插件被跳过不加载**（表现为游戏里完全没有小地图）。

## 验证

启动客户端后看 `BepInEx/LogOutput.log`：

- 应出现 `Loading [DynamicMaps 5.0BE ...]`
- 应出现 `DynamicMaps: N/N patch(es) applied`（无 not applied）
- 进 raid 后应出现 `Trying to attach peek component to BattleUI`
- 游戏内：右上角小地图；**按住 M** 看大地图（默认是"按住"模式，可在 `BepInEx/config/com.spt.dynamicmaps.cfg` 改为切换）

无 GUI 快速拉起（配合 `SPT_Runtime/SPT.Server.exe`）：
```bash
EscapeFromTarkov.exe -force-gfx-jobs native -token=<profileId> \
  -config={'BackendUrl':'https://127.0.0.1:6969','Version':'live','MatchingVersion':'live'}
```

### 日志里这些「错误」是无害的

排查时别被下面几条 `[Error]` 带偏（2026-10-11 实机 raid 日志核对过）：

| 日志 | 说明 |
| --- | --- |
| `[Error :Il2CppInterop] Assembly DynamicMaps.dll is not registered in il2cpp` | 每次开局固定出现 2 条。插件里的 enum（`LayerStatus` / `EMiniMapPosition` / `RotationAxis`）走 `Il2CppClassPointerStore` 查原生类时，按 `targetType.Module.Name`（即 `DynamicMaps.dll`）去 il2cpp 镜像表里找，而注入的 mono 程序集本就不在那张表里，于是记一条 Error 并返回 `IntPtr.Zero`。这些 enum 只在托管侧用，不参与原生封送。所有类型随后都正常 `Registered mono type ... in il2cpp domain`。 |
| `[Error :     Unity] NullReferenceException: ...`（raid 结束时刷屏） | 引擎自身的错误通道（`Unity` 前缀），**无托管堆栈**，紧跟在 `[3D WebView] TerminateBrowserProcess` / `Disabling AcousticMap` / `TryUnregister owner:RainController` 之后，属撤离结算与退出的拆解过程，非插件产生。 |
| `[Warning:DynamicMaps] SvgUtils: cached sprite was destroyed/invalid, reloading from disk: ...` | 缓存里的 sprite 在切场景时被 Unity 销毁了，插件检测到并重新从磁盘读 PNG —— 这是**兜底逻辑正常工作**的表现（`SvgUtils` 用 Unity 重载的 `==` 判断已销毁对象），后面紧跟 `SvgUtils: loaded sprite ...` 即为成功。 |
| `[Warning:     Unity] Unable to add characters to font asset [DON'T USE! SimSun-Chinese SDF] ...` | 引擎渲染中文字体资源的老问题（本次日志里占 5743 条），与插件无关。 |
| `[Warning:     Unity] Renderer 'shadow' is registered with more than one LODGroup ...` | 场景美术资源噪音。 |

判定插件健康的唯一标准：`DynamicMaps: 13 patch(es) applied`（且**没有** `not applied`），
`BepInEx/ErrorLog.log` 为空。


## 服务端

服务端插件做两件事：

1. `IModMetadata` 元数据（`ModGuid: com.mpstark.dynamicmaps`，`SptVersion: ~5.0.0`）——**必须**能通过 SPT 的版本校验，否则整个 mod 被拒绝加载。
2. `DynamicMapsServer : IOnLoad`（`OnLoadOrder.Preload`）——克隆 `ItemTpl.MAP_WOODS_PLAN` 生成 6 张地图道具（Ground Zero / Streets / Reserve / Labs / Lighthouse / Labyrinth），并推进 Therapist 的 assort；同时把 `config.json` 通过 `CustomStaticRouter` 暴露在 `GET /dynamicmaps/load`。

客户端读不到这个路由也不影响使用，只会记
`Server does not expose /dynamicmaps/load; default config will be used` 并用默认值。

### 构建

```bash
cd Server
dotnet build DynamicMapsServer.csproj -c Release
```

构建后自动部署到 `$(ServerDir)/user/mods/mpstark-dynamicmaps/`
（`ServerDir` 默认 `D:\SPT\Client.1.2.0.0.47965\SPT_Runtime`，可用
`-p:ServerDir=...` 覆盖）。部署内容= `DynamicMapsServer.dll` + `config.json` + `LICENSE`。

> 参考程序集直接从 `$(ServerDir)` 下的运行时 DLL 引用（`SPTarkov.Server.Core` /
> `SPTarkov.DI` / `SPTarkov.Common` / `SemanticVersioning` / `JetBrains.Annotations`），
> 因此编译出来的引用版本总是和实际加载它的服务端一致。游戏更新后只要 `ServerDir`
> 指对，不需要额外改引用。

### 验证

启动 `SPT_Runtime/SPT.Server.exe`，日志应出现：

- `模组：Dynamic Maps 版本5.0.0 (GUID: com.mpstark.dynamicmaps ... ) 已加载`
- 6 行 `Mod: com.mpstark.dynamicmaps added item: <tpl> to database`

再直接请求一次路由确认配置下发（服务端把响应体按游戏客户端的格式压缩了，
所以必须带 `responsecompressed: 0` 才能看到明文）：

```bash
curl -sk -H "responsecompressed: 0" https://127.0.0.1:6969/dynamicmaps/load
# {"AllowShowFriendlyPlayerMarkersInRaid":true,"AllowShowEnemyPlayerMarkersInRaid":true,...}
```

## 出发布包

```bash
bash tools/make-release.sh                    # 默认 GameDir=D:\SPT\Client.1.2.0.0.47965
bash tools/make-release.sh "D:\SPT\Client.xxx" "D:\SPT\Client.xxx\SPT_Runtime"
```

脚本会先 Release 构建两个项目，然后把产物按安装布局打包成：

```
releases/DynamicMaps-5.0BE/
├─ BepInEx/plugins/SPT-DynamicMaps/            客户端插件（含 Maps/、Markers/、LICENSE）
├─ SPT_Runtime/user/mods/mpstark-dynamicmaps/  服务端插件
├─ README.md                                   安装说明（来源 tools/RELEASE-README.md）
└─ ...
releases/DynamicMaps-5.0BE.zip
```

**打包来源是构建后自动部署的目录**（`BepInEx/plugins/SPT-DynamicMaps`、
`user/mods/mpstark-dynamicmaps`），不是手工从 `bin/` 挑文件 —— 这样发布包与实机验证过的安装
逐字节一致，不会漏文件或混进旧文件。

打包前脚本会先把部署目录里的 DLL 与**刚编译出的** `bin/Release` 产物做 `cmp` 断言，不一致就
直接报错退出（提示哪个目录是陈旧的）。这一步是必要的：`dotnet build` 增量空转时不会重新部署，
部署目录可能还留着**别处构建**的产物（换个目录构建、换过 GameDir 都会），而只比对「发布包 vs
部署目录」两边都是同一个陈旧文件，照样会通过。最后脚本打印三方 sha256（发布包 / bin / 部署目录），
三者应完全相同。

要点：

- 发布包**不含** `.pdb`（调试产物）。
- 发布包**不含** `EscapeFromTarkov_Data/Managed/`。
  老版本 1.2.1 需要 `Unity.VectorGraphics.dll` + `Unity.InternalAPIEngineBridge.003.dll`，
  因为它运行时渲染 SVG；5.0BE 的地图图层在构建期就预渲染成 PNG 了
  （见 `Plugin/Utils/SvgUtils.cs`：`.svg` 一律改读同名 `.png`），不再需要那两个 DLL。
- `releases/` 已加入 `.gitignore`（12 MB 二进制不入库），随时可用脚本重建。

