using Stunlock.Core;

namespace ProfuselyViolentProgression.WallopWarpers;

public static class PrefabGuids
{
    /// <summary>
    /// Interacting with an open-world waygate.
    /// </summary>
    public static PrefabGUID AB_Interact_UseWaypoint_Cast = new PrefabGUID(-402025199);

    /// <summary>
    /// Interacting with a castle waygate.
    /// </summary>
    public static PrefabGUID AB_Interact_UseWaypoint_Castle_Cast = new PrefabGUID(-1252882299);

    /// <summary>
    /// Looking at the map after interacting with a waygate.
    /// </summary>
    public static PrefabGUID AB_Interact_UseWaypoint_Blocked = new PrefabGUID(1189493948); // todo: block this during pvp? not necessary, but better ux
    
    public static PrefabGUID Buff_InCombat_PvPVampire = new PrefabGUID(697095869);
    public static PrefabGUID Buff_Waypoint_Travel = new PrefabGUID(150521246);
    public static PrefabGUID Buff_Waypoint_TravelEnd = new PrefabGUID(-1361133205);
    public static PrefabGUID Buff_General_Phasing = new PrefabGUID(-79611032);
}
