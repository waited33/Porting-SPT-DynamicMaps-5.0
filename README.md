Version:1.2.0.0.47965


# DynamicMaps 5.0BE — build

Fixes an issue where state from the previous raid was carried into the next one, plus several
problems that showed up during longer play sessions.

---

## English

### Added

**Icebreaker.** The Icebreaker map from the 1.2.0 client is now drawn by the mod: all 16 levels
(Control Room up to Bridge Roof) are included, and the level whose vertical range contains your
position is picked automatically. Three things worth knowing:

- Its artwork is by **TarkovBOT.eu** (via Tarkov.dev) and is **CC BY-NC-SA 4.0** — *not* the MIT
  licence the rest of the mod uses. Credits are in `Maps/map_and_data_credits.txt`.
- The vanilla client only ships paper maps for Factory, Customs, Woods, Shoreline, Sanatorium and
  Interchange, so the plans for every other map (Ground Zero, Streets, Reserve, Labs, Lighthouse,
  The Labyrinth and now Icebreaker) are created by the server part of this mod — including
  Icebreaker's **"Icebreaker plan map"**, which Therapist sells.
- With the default config the map always shows. If you turn on **Require a map in your inventory**
  then every map needs its item, so install the server part too — otherwise Icebreaker, and the
  other server-generated maps, will not show while that option is enabled.

## 中文

### 新增

**破冰船。** 1.2.0 客户端的破冰船地图现在由本模组绘制：包含全部 16 个楼层（从控制室到舰桥顶），
并按你所在高度自动选中对应楼层。有几点需要知道：

- 它的美术来自 **TarkovBOT.eu**（经 Tarkov.dev），授权是 **CC BY-NC-SA 4.0**，和本模组其余部分的
  MIT 不一样。署名见 `Maps/map_and_data_credits.txt`。
- 原版客户端只自带 Factory、Customs、Woods、Shoreline、Sanatorium、Interchange 六张纸质地图，
  其余每张图（Ground Zero、Streets、Reserve、Labs、Lighthouse、迷宫，以及现在的破冰船）的图纸都
  由本模组的服务端部分生成——破冰船的 **"Icebreaker plan map"**（治疗师出售）也是如此。
- 默认配置下地图始终显示。如果你打开了 **Require a map in your inventory**，那每张图都需要对应
  道具，请一并安装服务端部分；否则开着该选项时，破冰船以及其它由服务端生成的图不会显示。
