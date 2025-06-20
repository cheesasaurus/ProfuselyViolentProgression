using System.Text;
using BepInEx.Configuration;

namespace ProfuselyViolentProgression.SurgeSuppressor;

internal class SurgeSuppressorConfig
{
    internal ConfigEntry<bool> OnlyProtectPlayers;
    internal ConfigEntry<int> ThrottleIntervalMilliseconds;

    internal SurgeSuppressorConfig(ConfigFile configFile)
    {
        OnlyProtectPlayers = configFile.Bind("General", "OnlyProtectPlayers", true, "If true, only players will be protected. If false, both mobs and players will be protected.");
        ThrottleIntervalMilliseconds = configFile.Bind("General", "ThrottleIntervalMilliseconds", 250, "How long after a static shock, until a protected target can be shocked again.");
    }
}
