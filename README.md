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
