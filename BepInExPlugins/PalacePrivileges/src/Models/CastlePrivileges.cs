using System;

namespace ProfuselyViolentProgression.PalacePrivileges.Models;

public struct CastlePrivileges
{
    public MiscPrivs Misc { get; set; }
    public BuildPrivs Build { get; set; }
    public CraftPrivs Craft { get; set; }
    public DoorPrivs Door { get; set; }
    public PrisonerPrivs Prisoner { get; set; }
    public ServantPrivs Servant { get; set; }
    public TeleporterPrivs Teleporter { get; set; }
    public RedistributionPrivs Redistribution { get; set; }
    public ArenaPrivs Arena { get; set; }
    public ResearchPrivs Research { get; set; }

    public readonly bool Intersects(CastlePrivileges other)
    {
        return MiscPrivs.None != (Misc | other.Misc)
            || BuildPrivs.None != (Build | other.Build)
            || CraftPrivs.None != (Craft | other.Craft)
            || DoorPrivs.None != (Door | other.Door)
            || PrisonerPrivs.None != (Prisoner | other.Prisoner)
            || ServantPrivs.None != (Servant | other.Servant)
            || TeleporterPrivs.None != (Teleporter | other.Teleporter)
            || RedistributionPrivs.None != (Redistribution | other.Redistribution)
            || ArenaPrivs.None != (Arena | other.Arena)
            || ResearchPrivs.None != (Research | other.Research);
    }

    public readonly bool IsSupersetOf(CastlePrivileges other)
    {
        return other.Misc == (Misc & other.Misc)
            && other.Build == (Build & other.Build)
            && other.Craft == (Craft & other.Craft)
            && other.Door == (Door & other.Door)
            && other.Prisoner == (Prisoner & other.Prisoner)
            && other.Servant == (Servant & other.Servant)
            && other.Teleporter == (Teleporter & other.Teleporter)
            && other.Redistribution == (Redistribution & other.Redistribution)
            && other.Arena == (Arena & other.Arena)
            && other.Research == (Research & other.Research);
    }

    public readonly bool IsSubsetOf(CastlePrivileges other)
    {
        return Misc == (Misc & other.Misc)
            && Build == (Build & other.Build)
            && Craft == (Craft & other.Craft)
            && Door == (Door & other.Door)
            && Prisoner == (Prisoner & other.Prisoner)
            && Servant == (Servant & other.Servant)
            && Teleporter == (Teleporter & other.Teleporter)
            && Redistribution == (Redistribution & other.Redistribution)
            && Arena == (Arena & other.Arena)
            && Research == (Research & other.Research);
    }

    public static readonly CastlePrivileges None = new CastlePrivileges();

    public static readonly CastlePrivileges All = new CastlePrivileges
    {
        Misc = MiscPrivs.All,
        Build = BuildPrivs.All,
        Craft = CraftPrivs.All,
        Door = DoorPrivs.All,
        Prisoner = PrisonerPrivs.All,
        Servant = ServantPrivs.All,
        Teleporter = TeleporterPrivs.All,
        Redistribution = RedistributionPrivs.All,
        Arena = ArenaPrivs.All,
        Research = ResearchPrivs.All,
    };

    public static CastlePrivileges operator |(CastlePrivileges left, CastlePrivileges right)
    {
        var result = new CastlePrivileges();
        result.Misc = left.Misc | right.Misc;
        result.Build = left.Build | right.Build;
        result.Craft = left.Craft | right.Craft;
        result.Door = left.Door | right.Door;
        result.Prisoner = left.Prisoner | right.Prisoner;
        result.Servant = left.Servant | right.Servant;
        result.Teleporter = left.Teleporter | right.Teleporter;
        result.Redistribution = left.Redistribution | right.Redistribution;
        result.Arena = left.Arena | right.Arena;
        result.Research = left.Research | right.Research;
        return result;
    }

    public static CastlePrivileges operator &(CastlePrivileges left, CastlePrivileges right)
    {
        var result = new CastlePrivileges();
        result.Misc = left.Misc & right.Misc;
        result.Build = left.Build & right.Build;
        result.Craft = left.Craft & right.Craft;
        result.Door = left.Door & right.Door;
        result.Prisoner = left.Prisoner & right.Prisoner;
        result.Servant = left.Servant & right.Servant;
        result.Teleporter = left.Teleporter & right.Teleporter;
        result.Redistribution = left.Redistribution & right.Redistribution;
        result.Arena = left.Arena & right.Arena;
        result.Research = left.Research & right.Research;
        return result;
    }

    public static CastlePrivileges operator ~(CastlePrivileges operand)
    {
        var result = new CastlePrivileges();
        result.Misc = ~operand.Misc;
        result.Build = ~operand.Build;
        result.Craft = ~operand.Craft;
        result.Door = ~operand.Door;
        result.Prisoner = ~operand.Prisoner;
        result.Servant = ~operand.Servant;
        result.Teleporter = ~operand.Teleporter;
        result.Redistribution = ~operand.Redistribution;
        result.Arena = ~operand.Arena;
        result.Research = ~operand.Research;
        return result;
    }
    
}

[Flags]
public enum MiscPrivs : long
{
    None = 0,
    All = -1,
    Lockbox = 1 << 1,
    Musicbox = 1 << 2,
    PlantSeeds = 1 << 3,
}

[Flags]
public enum BuildPrivs : long
{
    None = 0,
    All = -1,
    UseTreasury = 1,
}

[Flags]
public enum CraftPrivs : long
{
    None = 0,
    All = -1,
    UseTreasury = 1,
    AccessStations = 1 << 1,
    ToggleRecipes = 1 << 2,
}

[Flags]
public enum ResearchPrivs : long
{
    None = 0,
    All = -1,
    DeskTier1 = 1,
    DeskTier2 = 1 << 1,
    DeskTier3 = 1 << 2,
    AltarStygian = 1 << 3,
}

[Flags]
public enum DoorPrivs : long
{
    None = 0,
    All = -1,
    ServantLocked = 1, // !Door.CanBeOpenedByServant
    NotServantLocked = 1 << 1, // Door.CanBeOpenedByServant
    ThinFence = 1 << 2,
    ThinPalisade = 1 << 3, // PalisadesBuildMenuGroup ? TM_Castle_Wall_Door_Palisade_Tier01 ?
    ThinBasic = 1 << 4, // DoorTier02BuildMenuGroup ?

    // TM_Castle_Wall_Door_Wood_Tier02_Standard01_Standard PrefabGUID(-1720487003)
    // TM_Castle_Wall_Door_Wood_Tier02_Standard01_ServantLock PrefabGUID(661164434)
    ThinBattlement = 1 << 5, // DoorTier02Standard01BuildMenuGroup ? 
    ThinCordial = 1 << 6, // DoorTier02Standard02BuildMenuGroup ?
    ThinVerdant = 1 << 7, // DoorTier02Standard03BuildMenuGroup ?
    ThinRoyal = 1 << 8, // DoorTier02_DLC01_DraculasRelic_BuildMenuGroup ?
    ThinPlagueSanctum = 1 << 9, // DoorTier02_DLC02_Gloomrot_BuildMenuGroup ?
    ThinAncientSymphony1 = 1 << 10, // DoorTier02_DLC_ProjectK01_BuildMenuGroup ?
    ThinAncientSymphony2 = 1 << 11, // DoorTier02_DLC_ProjectK02_BuildMenuGroup ?
    ThinNocturnalOpulence = 1 << 12, // DoorTier02_DLC_StrongbladeDLC01_BuildMenuGroup ?
    ThinNocturnalOpulenceRed = 1 << 13,
    ThinNocturnalOpulenceOrange = 1 << 14,
    ThinNocturnalOpulenceYellow = 1 << 15,
    ThinNocturnalOpulenceGreen = 1 << 16,
    ThinNocturnalOpulenceGreenLight = 1 << 17, // todo: actual color name. light green
    ThinNocturnalOpulenceBlueLight = 1 << 18, // todo: actual color name. light blue
    ThinNocturnalOpulenceBlue = 1 << 19,
    ThinNocturnalOpulencePurple = 1 << 20,
    ThinNocturnalOpulencePink = 1 << 21,
    ThinNocturnalOpulenceWhite = 1 << 22,
    ThinNocturnalOpulenceGrey = 1 << 23,
    ThinNocturnalOpulenceBlack = 1 << 24,
    ThinPrison = 1 << 25, // DoorTier02_PrisonStyle01_BuildMenuGroup ?
    ThinBarrier = 1 << 26, // DoorTier02_PrisonStyle02_BuildMenuGroup ?
    WideBars = 1 << 27, // DoorTier02_Wide_01_BuildMenuGroup ?
    WidePlanks = 1 << 28, // DoorTier02_Wide_02_BuildMenuGroup ?

    Thin = ThinFence | ThinPalisade | ThinBattlement | ThinCordial | ThinVerdant
        | ThinRoyal | ThinPlagueSanctum | ThinAncientSymphony1 | ThinAncientSymphony2
        | ThinNocturnalOpulence | ThinNocturnalOpulenceRed | ThinNocturnalOpulenceOrange
        | ThinNocturnalOpulenceYellow | ThinNocturnalOpulenceGreen | ThinNocturnalOpulenceGreenLight
        | ThinNocturnalOpulenceBlueLight | ThinNocturnalOpulenceBlue | ThinNocturnalOpulencePurple
        | ThinNocturnalOpulencePink | ThinNocturnalOpulenceWhite | ThinNocturnalOpulenceGrey
        | ThinNocturnalOpulenceBlack
        | ThinPrison | ThinBarrier,

    Wide = WideBars | WidePlanks,
}

[Flags]
public enum PrisonerPrivs : long
{
    None = 0,
    All = -1,
    Subdue = 1,
    Kill = 1 << 1,
    ExtractBlood = 1 << 2,
    FeedSafeFood = 1 << 3,
    FeedUnSafeFood = 1 << 4,
}

[Flags]
public enum ServantPrivs : long
{
    None = 0,
    All = -1,
    Convert = 1,
    Terminate = 1 << 1,
    Gear = 1 << 2,
    Rename = 1 << 3,
    Throne = 1 << 4,
}

[Flags]
public enum TeleporterPrivs : long
{
    None = 0,
    All = -1,
    WaygateOut = 1,
    WaygateIn = 1 << 1,
    Waygate = WaygateIn | WaygateOut,
    Red = 1 << 2,
    Yellow = 1 << 3,
    Purple = 1 << 4,
    Blue = 1 << 5,
    AllSmall = Red | Yellow | Purple | Blue,
}

[Flags]
public enum RedistributionPrivs : long
{
    None = 0,
    All = -1,
    QuickSend = 1,
    ToggleAutoSend = 1 << 1,
    Edit = 1 << 2,
}

[Flags]
public enum ArenaPrivs : long
{
    None = 0,
    All = -1,
    StartContest = 1,
    EditRules = 1 << 1,
    ZonePainting = 1 << 2,
}
