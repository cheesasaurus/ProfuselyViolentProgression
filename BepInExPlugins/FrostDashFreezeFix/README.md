# FrostDashFreezeFix

Patches exploitable dash jewel bugs.

### Veil of Frost

The "consume chill and inflict freeze" jewel could freeze targets that weren't chilled.

![frost dash freeze jewel](https://github.com/cheesasaurus/ProfuselyViolentProgression/raw/main/BepInExPlugins/FrostDashFreezeFix/images/frost-dash-freeze-jewel.png)

### Veil of Blood

The "hitting an enemy affected by leech" jewel could grant empower when hitting targets that weren't leeched.

![frost dash freeze jewel](https://github.com/cheesasaurus/ProfuselyViolentProgression/raw/main/BepInExPlugins/FrostDashFreezeFix/images/blood-dash-empower-jewel.png)

### Veil of Illusion

The "hitting an enemy affected by weaken" jewel could grant a shield when hitting targets that weren't weakened.

![frost dash freeze jewel](https://github.com/cheesasaurus/ProfuselyViolentProgression/raw/main/BepInExPlugins/FrostDashFreezeFix/images/illusion-dash-shield-jewel.png)

### Other dashes

I'm not aware of other exploits, but FrostDashFreezeFix also does corrective checks for:
- `AB_Vampire_VeilOfStorm_TriggerBonusEffects` with `SpellMod_Shared_Storm_ConsumeStaticIntoStun`
- `AB_Vampire_VeilOfChaos_TriggerBonusEffects` with `SpellMod_Shared_Chaos_ConsumeIgniteAgonizingFlames_OnAttack`

## Installation

- Install [BepInEx](https://v-rising.thunderstore.io/package/BepInEx/BepInExPack_V_Rising/).
- Install [HookDOTS](https://thunderstore.io/c/v-rising/p/cheesasaurus/HookDOTS_API/).
- Extract _FrostDashFreezeFix.dll_ into _`(VRising folder)/BepInEx/plugins`_.

## Support

Join the [modding community](https://vrisingmods.com/discord).

Post an issue on the [GitHub repository](https://github.com/cheesasaurus/ProfuselyViolentProgression). 