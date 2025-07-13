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

    public readonly bool Intersects(CastlePrivileges other)
    {
        return MiscPrivs.None != (Misc & other.Misc)
            || BuildPrivs.None != (Build & other.Build)
            || CraftPrivs.None != (Craft & other.Craft)
            || DoorPrivs.None != (Door & other.Door)
            || PrisonerPrivs.None != (Prisoner & other.Prisoner)
            || ServantPrivs.None != (Servant & other.Servant)
            || TeleporterPrivs.None != (Teleporter & other.Teleporter)
            || RedistributionPrivs.None != (Redistribution & other.Redistribution)
            || ArenaPrivs.None != (Arena & other.Arena);
    }

    public readonly bool IsSuperset(CastlePrivileges other)
    {
        return other.Misc == (Misc & other.Misc)
            && other.Build == (Build & other.Build)
            && other.Craft == (Craft & other.Craft)
            && other.Door == (Door & other.Door)
            && other.Prisoner == (Prisoner & other.Prisoner)
            && other.Servant == (Servant & other.Servant)
            && other.Teleporter == (Teleporter & other.Teleporter)
            && other.Redistribution == (Redistribution & other.Redistribution)
            && other.Arena == (Arena & other.Arena);
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
    Throne = 1 << 3,
    Research = 1 << 4,
    PlantSeeds = 1 << 5,
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
public enum DoorPrivs : long
{
    None = 0,
    All = -1,
    ServantLocked = 1, // !Door.CanBeOpenedByServant
    NotServantLocked = 1 << 1, // Door.CanBeOpenedByServant
    Thin = 1 << 2, // to future-proof, we will do a separate check for thin/wide, rather than combining bits
    Wide = 1 << 3, // to future-proof, we will do a separate check for thin/wide, rather than combining bits
    ThinFence = 1 << 4,
    ThinPalisade = 1 << 5, // PalisadesBuildMenuGroup ? TM_Castle_Wall_Door_Palisade_Tier01 ?
    ThinBasic = 1 << 6, // DoorTier02BuildMenuGroup ?
    ThinBattlement = 1 << 7, // DoorTier02Standard01BuildMenuGroup ?
    ThinCordial = 1 << 8, // DoorTier02Standard02BuildMenuGroup ?
    ThinVerdant = 1 << 9, // DoorTier02Standard03BuildMenuGroup ?
    ThinRoyal = 1 << 10, // DoorTier02_DLC01_DraculasRelic_BuildMenuGroup ?
    ThinPlagueSanctum = 1 << 11, // DoorTier02_DLC02_Gloomrot_BuildMenuGroup ?
    ThinAncientSymphony1 = 1 << 12, // DoorTier02_DLC_ProjectK01_BuildMenuGroup ?
    ThinAncientSymphony2 = 1 << 13, // DoorTier02_DLC_ProjectK02_BuildMenuGroup ?
    ThinNocturnalOpulence = 1 << 14, // DoorTier02_DLC_StrongbladeDLC01_BuildMenuGroup ?
    ThinNocturnalOpulenceRed = 1 << 15,
    ThinNocturnalOpulenceOrange = 1 << 16,
    ThinNocturnalOpulenceYellow = 1 << 17,
    ThinNocturnalOpulenceGreen = 1 << 18,
    ThinNocturnalOpulenceGreenLight = 1 << 19, // todo: actual color name. light green
    ThinNocturnalOpulenceBlueLight = 1 << 20, // todo: actual color name. light blue
    ThinNocturnalOpulenceBlue = 1 << 21,
    ThinNocturnalOpulencePurple = 1 << 22,
    ThinNocturnalOpulencePink = 1 << 23,
    ThinNocturnalOpulenceWhite = 1 << 24,
    ThinNocturnalOpulenceGrey = 1 << 25,
    ThinNocturnalOpulenceBlack = 1 << 26,
    ThinPrison = 1 << 27, // DoorTier02_PrisonStyle01_BuildMenuGroup ?
    ThinBarrier = 1 << 28, // DoorTier02_PrisonStyle02_BuildMenuGroup ?
    WideBars = 1 << 29, // DoorTier02_Wide_01_BuildMenuGroup ?
    WidePlanks = 1 << 30, // DoorTier02_Wide_02_BuildMenuGroup ?
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
}

[Flags]
public enum TeleporterPrivs : long
{
    None = 0,
    All = -1,
    Waygates = 1,
    Red = 1 << 1,
    Yellow = 1 << 2,
    Purple = 1 << 3,
    Blue = 1 << 4,
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
