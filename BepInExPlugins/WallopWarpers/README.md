# WallopWarpers

Prevents waygate use during PvP combat.

Provides better protection after teleporting to a waygate, to mitigate camping.
- Longer lasting "Phasing" protection buff.
- Allows using the waygate while Phasing.

All configurable.


## Installation

- Install [BepInEx](https://v-rising.thunderstore.io/package/BepInEx/BepInExPack_V_Rising/).
- Install [HookDOTS](https://thunderstore.io/c/v-rising/p/cheesasaurus/HookDOTS_API/).
- Extract `WallopWarpers.dll` into _`(VRising folder)/BepInEx/plugins`_.

### Configuration

The following configuration settings are available in `BepInEx/config/WallopWarpers.cfg`.

- `PvPCombat_AllowWaygateUse` [default `false`]: Whether or not to allow waygate use during PvP combat.
- `SpawnProtectionSeconds` [default `12`]: How long the protection buff lasts, after teleporting in.
- `SpawnProtection_AllowWaygateUse` [default `true`]: Whether or not to allow waygate use during protection.

Configuration changes are automatically applied; no need to restart the server every time.

### Support

Join the [modding community](https://vrisingmods.com/discord).

Post an issue on the [GitHub repository](https://github.com/cheesasaurus/ProfuselyViolentProgression). 