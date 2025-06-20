# SurgeSuppressor

Limits how frequently a target can be shocked by static.

![static shock definition](https://github.com/cheesasaurus/ProfuselyViolentProgression/raw/main/BepInExPlugins/SurgeSuppressor/images/static-shock-definition.png)


### Installation

- Install [BepInEx](https://v-rising.thunderstore.io/package/BepInEx/BepInExPack_V_Rising/).
- Install [HookDOTS](https://thunderstore.io/c/v-rising/p/cheesasaurus/HookDOTS_API/).
- Extract _SurgeSuppressor.dll_ into _`(VRising folder)/BepInEx/plugins`_.

### Configuration

The following configuration settings are available in `BepInEx/config/SurgeSuppressor.cfg`.

- `OnlyProtectPlayers` [default `true`]: If true, only players will be protected. If false, both mobs and players will be protected.
- `ThrottleIntervalMilliseconds` [default `250`]: How long after a static shock, until a protected target can be shocked again.

### Support

Join the [modding community](https://vrisingmods.com/discord).

Post an issue on the [GitHub repository](https://github.com/cheesasaurus/ProfuselyViolentProgression). 