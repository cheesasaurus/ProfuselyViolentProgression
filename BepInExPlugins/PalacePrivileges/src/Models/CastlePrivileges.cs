using System;
using System.Collections.Generic;
using System.Text;

namespace ProfuselyViolentProgression.PalacePrivileges.Models;

public struct CastlePrivileges
{
    public MiscPrivs Misc { get; set; }
    public BuildPrivs Build { get; set; }
    public CraftPrivs Craft { get; set; }
    public DoorPrivs Door { get; set; }
    public PrisonPrivs Prison { get; set; }
    public ServantPrivs Servant { get; set; }
    public TeleporterPrivs Teleporter { get; set; }
    public RedistributionPrivs Redistribution { get; set; }
    public ArenaPrivs Arena { get; set; }
    public ResearchPrivs Research { get; set; }
    public SowSeedPrivs SowSeed { get; set; }
    public PlantTreePrivs PlantTree { get; set; }

    public readonly bool Intersects(CastlePrivileges other)
    {
        return MiscPrivs.None != (Misc & other.Misc)
            || BuildPrivs.None != (Build & other.Build)
            || CraftPrivs.None != (Craft & other.Craft)
            || DoorPrivs.None != (Door & other.Door)
            || PrisonPrivs.None != (Prison & other.Prison)
            || ServantPrivs.None != (Servant & other.Servant)
            || TeleporterPrivs.None != (Teleporter & other.Teleporter)
            || RedistributionPrivs.None != (Redistribution & other.Redistribution)
            || ArenaPrivs.None != (Arena & other.Arena)
            || ResearchPrivs.None != (Research & other.Research)
            || SowSeedPrivs.None != (SowSeed & other.SowSeed)
            || PlantTreePrivs.None != (PlantTree & other.PlantTree);
    }

    public readonly bool IsSupersetOf(CastlePrivileges other)
    {
        return other.Misc == (Misc & other.Misc)
            && other.Build == (Build & other.Build)
            && other.Craft == (Craft & other.Craft)
            && other.Door == (Door & other.Door)
            && other.Prison == (Prison & other.Prison)
            && other.Servant == (Servant & other.Servant)
            && other.Teleporter == (Teleporter & other.Teleporter)
            && other.Redistribution == (Redistribution & other.Redistribution)
            && other.Arena == (Arena & other.Arena)
            && other.Research == (Research & other.Research)
            && other.SowSeed == (SowSeed & other.SowSeed)
            && other.PlantTree == (PlantTree & other.PlantTree);
    }

    public readonly bool IsSubsetOf(CastlePrivileges other)
    {
        return Misc == (Misc & other.Misc)
            && Build == (Build & other.Build)
            && Craft == (Craft & other.Craft)
            && Door == (Door & other.Door)
            && Prison == (Prison & other.Prison)
            && Servant == (Servant & other.Servant)
            && Teleporter == (Teleporter & other.Teleporter)
            && Redistribution == (Redistribution & other.Redistribution)
            && Arena == (Arena & other.Arena)
            && Research == (Research & other.Research)
            && SowSeed == (SowSeed & other.SowSeed)
            && PlantTree == (PlantTree & other.PlantTree);
    }

    public static readonly CastlePrivileges None = new CastlePrivileges();

    public static readonly CastlePrivileges All = new CastlePrivileges
    {
        Misc = MiscPrivs.All,
        Build = BuildPrivs.All,
        Craft = CraftPrivs.All,
        Door = DoorPrivs.All,
        Prison = PrisonPrivs.All,
        Servant = ServantPrivs.All,
        Teleporter = TeleporterPrivs.All,
        Redistribution = RedistributionPrivs.All,
        Arena = ArenaPrivs.All,
        Research = ResearchPrivs.All,
        SowSeed = SowSeedPrivs.All,
        PlantTree = PlantTreePrivs.All,
    };

    public static CastlePrivileges operator |(CastlePrivileges left, CastlePrivileges right)
    {
        var result = new CastlePrivileges();
        result.Misc = left.Misc | right.Misc;
        result.Build = left.Build | right.Build;
        result.Craft = left.Craft | right.Craft;
        result.Door = left.Door | right.Door;
        result.Prison = left.Prison | right.Prison;
        result.Servant = left.Servant | right.Servant;
        result.Teleporter = left.Teleporter | right.Teleporter;
        result.Redistribution = left.Redistribution | right.Redistribution;
        result.Arena = left.Arena | right.Arena;
        result.Research = left.Research | right.Research;
        result.SowSeed = left.SowSeed | right.SowSeed;
        result.PlantTree = left.PlantTree | right.PlantTree;
        return result;
    }

    public static CastlePrivileges operator &(CastlePrivileges left, CastlePrivileges right)
    {
        var result = new CastlePrivileges();
        result.Misc = left.Misc & right.Misc;
        result.Build = left.Build & right.Build;
        result.Craft = left.Craft & right.Craft;
        result.Door = left.Door & right.Door;
        result.Prison = left.Prison & right.Prison;
        result.Servant = left.Servant & right.Servant;
        result.Teleporter = left.Teleporter & right.Teleporter;
        result.Redistribution = left.Redistribution & right.Redistribution;
        result.Arena = left.Arena & right.Arena;
        result.Research = left.Research & right.Research;
        result.SowSeed = left.SowSeed & right.SowSeed;
        result.PlantTree = left.PlantTree & right.PlantTree;
        return result;
    }

    public static CastlePrivileges operator ~(CastlePrivileges operand)
    {
        var result = new CastlePrivileges();
        result.Misc = ~operand.Misc;
        result.Build = ~operand.Build;
        result.Craft = ~operand.Craft;
        result.Door = ~operand.Door;
        result.Prison = ~operand.Prison;
        result.Servant = ~operand.Servant;
        result.Teleporter = ~operand.Teleporter;
        result.Redistribution = ~operand.Redistribution;
        result.Arena = ~operand.Arena;
        result.Research = ~operand.Research;
        result.SowSeed = ~operand.SowSeed;
        result.PlantTree = ~operand.PlantTree;
        return result;
    }

    public override string ToString()
    {
        var privNames = new List<string>();

        if (Misc != MiscPrivs.None)
        {
            privNames.Add($"Misc: {Misc}");
        }
        if (Build != BuildPrivs.None)
        {
            privNames.Add($"Build: {Build}");
        }
        if (Craft != CraftPrivs.None)
        {
            privNames.Add($"Craft: {Craft}");
        }
        if (Door != DoorPrivs.None)
        {
            privNames.Add($"Door: {Door}");
        }
        if (Prison != PrisonPrivs.None)
        {
            privNames.Add($"Prisoner: {Prison}");
        }
        if (Servant != ServantPrivs.None)
        {
            privNames.Add($"Servant: {Servant}");
        }
        if (Teleporter != TeleporterPrivs.None)
        {
            privNames.Add($"Teleporter: {Teleporter}");
        }
        if (Redistribution != RedistributionPrivs.None)
        {
            privNames.Add($"Redistribution: {Redistribution}");
        }
        if (Arena != ArenaPrivs.None)
        {
            privNames.Add($"Arena: {Arena}");
        }
        if (Research != ResearchPrivs.None)
        {
            privNames.Add($"Research: {Research}");
        }
        if (SowSeed != SowSeedPrivs.None)
        {
            privNames.Add($"SowSeed: {Research}");
        }
        if (PlantTree != PlantTreePrivs.None)
        {
            privNames.Add($"PlantTree: {Research}");
        }

        return string.Join(" | ", privNames);
    }
    
}

[Flags]
public enum MiscPrivs : long
{
    None = 0,
    All = -1,
    Lockbox = 1 << 1,
    Musicbox = 1 << 2,
    RenameStructures = 1 << 3,
    ToggleRefinement = 1 << 4,
}

[Flags]
public enum BuildPrivs : long
{
    None = 0,
    All = -1,
    UnlistedTBD = 1,
    UseTreasury = 1 << 1,
}

[Flags]
public enum CraftPrivs : long
{
    None = 0,
    All = -1,
    UseTreasury = 1,
    CraftItem = 1 << 1,
}

[Flags]
public enum ResearchPrivs : long
{
    None = 0,
    All = -1,
    DeskTier1 = 1,
    DeskTier2 = 1 << 1,
    DeskTier3 = 1 << 2,
    StygianAltar = 1 << 3,
    UseTreasury = 1 << 4,
}

[Flags]
public enum DoorPrivs : long
{
    None = 0,
    All = -1,
    ServantLocked = 1, // !Door.CanBeOpenedByServant
    NotServantLocked = 1 << 1, // Door.CanBeOpenedByServant
    ThinFence = 1 << 2,
    ThinPalisade = 1 << 3,
    ThinBasic = 1 << 4,
    ThinBattlement = 1 << 5,
    ThinCordial = 1 << 6,
    ThinVerdant = 1 << 7,
    ThinRoyal = 1 << 8,
    ThinPlagueSanctum = 1 << 9,
    ThinAncientSymphony1 = 1 << 10,
    ThinAncientSymphony2 = 1 << 11,
    ThinNocturnalOpulence = 1 << 12,
    DyedRed = 1 << 13,
    DyedOrange = 1 << 14,
    DyedYellow = 1 << 15,
    DyedGreen = 1 << 16,
    DyedMintGreen = 1 << 17,
    DyedCyan = 1 << 18,
    DyedBlue = 1 << 19,
    DyedPurple = 1 << 20,
    DyedPink = 1 << 21,
    DyedWhite = 1 << 22,
    DyedGrey = 1 << 23,
    DyedBlack = 1 << 24,
    ThinPrison = 1 << 25,
    ThinBarrier = 1 << 26,
    WideBars = 1 << 27,
    WidePlanks = 1 << 28,

    Thin = ThinFence | ThinPalisade | ThinBasic | ThinBattlement | ThinCordial | ThinVerdant
        | ThinRoyal | ThinPlagueSanctum | ThinAncientSymphony1 | ThinAncientSymphony2
        | ThinNocturnalOpulence | DyedRed | DyedOrange
        | DyedYellow | DyedGreen | DyedMintGreen
        | DyedCyan | DyedBlue | DyedPurple
        | DyedPink | DyedWhite | DyedGrey
        | DyedBlack
        | ThinPrison | ThinBarrier,

    Wide = WideBars | WidePlanks,
}

[Flags]
public enum PrisonPrivs : long
{
    None = 0,
    All = -1,
    Subdue = 1,
    Kill = 1 << 1,
    ExtractBlood = 1 << 2,
    FeedSafeFood = 1 << 3,
    FeedUnSafeFood = 1 << 4,
    UseTreasury = 1 << 5,
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
    UseStation = 1,
    ZonePainting = 1 << 1,
}

[Flags]
public enum SowSeedPrivs : long
{
    None = 0,
    All = -1,
    BloodRose = 1,
    FireBlossom = 1 << 1,
    SnowFlower = 1 << 2,
    HellsClarion = 1 << 3,
    MourningLily = 1 << 4,
    Sunflower = 1 << 5,
    PlagueBrier = 1 << 6,
    Grapes = 1 << 7,
    CorruptedFlower = 1 << 8,
    BleedingHeart = 1 << 9,
    GhostShroom = 1 << 10,
    TrippyShroom = 1 << 11,
    Cotton = 1 << 12,
    Thistle = 1 << 13,
}

[Flags]
public enum PlantTreePrivs : long
{
    None = 0,
    All = -1,
    Pine = 1,
    Cypress = 1 << 1,
    Aspen = 1 << 2,
    AspenAutumn = 1 << 3,
    Birch = 1 << 4,
    BirchAutumn = 1 << 5,
    Apple = 1 << 6,
    Cursed = 1 << 7,
    Gloomy = 1 << 8,
    Cherry = 1 << 9,
    CherryWhite = 1 << 10,
    Oak = 1 << 11,
}
