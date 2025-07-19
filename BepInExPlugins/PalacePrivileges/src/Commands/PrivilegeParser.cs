using System;
using System.Collections.Generic;
using ProfuselyViolentProgression.Core.Utilities;
using ProfuselyViolentProgression.PalacePrivileges.Models;

namespace ProfuselyViolentProgression.PalacePrivileges.Commands;

public class PrivilegeParser
{
    protected Dictionary<string, NamedCastlePrivileges> PrivsLookup = [];

    public PrivilegeParser()
    {
        InitPrivsLookup();
    }

    public struct ParseResult()
    {
        public CastlePrivileges Privs;
        public List<string> ValidPrivNames { get; set; } = [];
        public List<string> InvalidPrivNames { get; set; } = [];
    }

    public ParseResult ParsePrivilegesFromCommandString(string str)
    {
        var result = new ParseResult();
        var privNames = str.Split(" ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var privName in privNames)
        {
            var lookupKey = privName.ToLowerInvariant();
            if (PrivsLookup.TryGetValue(lookupKey, out var namedPrivs))
            {
                result.ValidPrivNames.Add(namedPrivs.Name);
                result.Privs |= namedPrivs.Privs;
            }
            else
            {
                result.InvalidPrivNames.Add(privName);
            }
        }
        return result;
    }

    public List<string> PrivilegeNames(CastlePrivileges privs)
    {
        // todo: filter out redundant privileges.
        // e.g. "cat.all cat.some cat.1 cat.2"
        // should simply be "cat.all"
        // Needs some thought; the obvious way would not scale well
        //
        // idea: for each group, order by largest flag value first.
        // then while extracting from that, check if each added thing is a superset before adding.
        // but still doesn't scale great.
        //
        // maybe something else we can do with flags, like creating a master flags to check.
        // when something is added, turn on its bits in the master flags.
        // And only check the master flags when extracting.
        // But not sure that would be correct; need to think about it.
        //
        // Probably needs some preprocessing
        var names = new List<string>();
        
        foreach (var (key, namedPriv) in PrivsLookup)
        {
            if (namedPriv.Privs.IsSubsetOf(privs))
            {
                names.Add(namedPriv.Name);
            }
        }
        return names;
    }

    public Dictionary<string, List<string>> PrivilegeNamesGrouped(CastlePrivileges privs)
    {
        var groupedNames = new Dictionary<string, List<string>>();
        groupedNames.Add("", []);

        foreach (var (key, namedPriv) in PrivsLookup)
        {
            if (namedPriv.Privs.IsSubsetOf(privs))
            {
                if (!groupedNames.ContainsKey(namedPriv.Prefix))
                {
                    groupedNames.Add(namedPriv.Prefix, []);
                }
                groupedNames[namedPriv.Prefix].Add(namedPriv.Name);
            }
        }
        return groupedNames;
    }

    private void RegisterPrivs(string name, CastlePrivileges privs)
    {
        var indexOfDot = name.IndexOf(".");
        var prefix = indexOfDot == -1 ? "" : name.Substring(0, indexOfDot);

        PrivsLookup.Add(name.ToLowerInvariant(), new()
        {
            Prefix = prefix,
            Name = name,
            Privs = privs,
        });
    }

    private void RegisterPrivs(string name, MiscPrivs privs)
    {
        RegisterPrivs(name, new CastlePrivileges
        {
            Misc = privs,
        });
    }

    private void RegisterPrivs(string name, BuildPrivs privs)
    {
        RegisterPrivs(name, new CastlePrivileges
        {
            Build = privs,
        });
    }

    private void RegisterPrivs(string name, CraftPrivs privs)
    {
        RegisterPrivs(name, new CastlePrivileges
        {
            Craft = privs,
        });
    }

    private void RegisterPrivs(string name, DoorPrivs privs)
    {
        RegisterPrivs(name, new CastlePrivileges
        {
            Door = privs,
        });
    }

    private void RegisterPrivs(string name, PrisonPrivs privs)
    {
        RegisterPrivs(name, new CastlePrivileges
        {
            Prison = privs,
        });
    }

    private void RegisterPrivs(string name, ServantPrivs privs)
    {
        RegisterPrivs(name, new CastlePrivileges
        {
            Servant = privs,
        });
    }

    private void RegisterPrivs(string name, TeleporterPrivs privs)
    {
        RegisterPrivs(name, new CastlePrivileges
        {
            Teleporter = privs,
        });
    }

    private void RegisterPrivs(string name, RedistributionPrivs privs)
    {
        RegisterPrivs(name, new CastlePrivileges
        {
            Redistribution = privs,
        });
    }

    private void RegisterPrivs(string name, ArenaPrivs privs)
    {
        RegisterPrivs(name, new CastlePrivileges
        {
            Arena = privs,
        });
    }

    private void RegisterPrivs(string name, ResearchPrivs privs)
    {
        RegisterPrivs(name, new CastlePrivileges
        {
            Research = privs,
        });
    }
    
    private void RegisterPrivs(string name, SowSeedPrivs privs)
    {
        RegisterPrivs(name, new CastlePrivileges
        {
            SowSeed = privs,
        });
    }

    private void RegisterPrivs(string name, PlantTreePrivs privs)
    {
        RegisterPrivs(name, new CastlePrivileges
        {
            PlantTree = privs,
        });
    }

    private void InitPrivsLookup()
    {
        // todo: comment out anything not functional before release

        RegisterPrivs("all", CastlePrivileges.All);

        RegisterPrivs("lockbox", MiscPrivs.Lockbox);
        RegisterPrivs("musicbox", MiscPrivs.Musicbox); // planned feature
        RegisterPrivs("renameStructures", MiscPrivs.RenameStructures);

        RegisterPrivs("build.all", BuildPrivs.All);
        RegisterPrivs("build.useTreasury", BuildPrivs.All);

        RegisterPrivs("craft.all", CraftPrivs.All);
        RegisterPrivs("craft.useTreasury", CraftPrivs.UseTreasury);
        RegisterPrivs("craft.accessStations", CraftPrivs.AccessStations); // planned feature
        RegisterPrivs("craft.toggleRecipes", CraftPrivs.ToggleRecipes); // planned feature

        RegisterPrivs("prisoners.all", PrisonPrivs.All);
        RegisterPrivs("prisoners.subdue", PrisonPrivs.Subdue);
        RegisterPrivs("prisoners.kill", PrisonPrivs.Kill);
        RegisterPrivs("prisoners.extractBlood", PrisonPrivs.ExtractBlood);
        RegisterPrivs("prisoners.feedSafeFood", PrisonPrivs.FeedSafeFood);
        RegisterPrivs("prisoners.feedUnSafeFood", PrisonPrivs.FeedUnSafeFood);
        RegisterPrivs("prisoners.useTreasury", PrisonPrivs.FeedUnSafeFood);

        RegisterPrivs("servants.all", ServantPrivs.All);
        RegisterPrivs("servants.convert", ServantPrivs.Convert);
        RegisterPrivs("servants.terminate", ServantPrivs.Terminate);
        RegisterPrivs("servants.gear", ServantPrivs.Gear);
        RegisterPrivs("servants.rename", ServantPrivs.Rename);
        RegisterPrivs("servants.throne", ServantPrivs.Throne); // planned feature

        RegisterPrivs("tp.all", TeleporterPrivs.All);
        RegisterPrivs("tp.waygate", TeleporterPrivs.Waygate);
        RegisterPrivs("tp.waygateOut", TeleporterPrivs.WaygateOut);
        RegisterPrivs("tp.waygateIn", TeleporterPrivs.WaygateIn);
        RegisterPrivs("tp.red", TeleporterPrivs.Red);
        RegisterPrivs("tp.yellow", TeleporterPrivs.Yellow);
        RegisterPrivs("tp.purple", TeleporterPrivs.Purple);
        RegisterPrivs("tp.blue", TeleporterPrivs.Blue);
        RegisterPrivs("tp.allSmall", TeleporterPrivs.AllSmall);

        RegisterPrivs("redist.all", RedistributionPrivs.All); // planned feature
        RegisterPrivs("redist.quickSend", RedistributionPrivs.QuickSend); // planned feature
        RegisterPrivs("redist.toggleAutoSend", RedistributionPrivs.ToggleAutoSend); // planned feature
        RegisterPrivs("redist.edit", RedistributionPrivs.Edit); // planned feature

        RegisterPrivs("arena.all", ArenaPrivs.All); // planned feature
        RegisterPrivs("arena.startContest", ArenaPrivs.StartContest); // planned feature
        RegisterPrivs("arena.editRules", ArenaPrivs.EditRules); // planned feature
        RegisterPrivs("arena.zonePainting", ArenaPrivs.ZonePainting); // planned feature

        RegisterPrivs("research.all", ResearchPrivs.All); // planned feature
        RegisterPrivs("research.t1", ResearchPrivs.DeskTier1); // planned feature
        RegisterPrivs("research.t2", ResearchPrivs.DeskTier2); // planned feature
        RegisterPrivs("research.t3", ResearchPrivs.DeskTier3); // planned feature
        RegisterPrivs("research.stygian", ResearchPrivs.AltarStygian); // planned feature
        RegisterPrivs("research.useTreasury", ResearchPrivs.UseTreasury); // planned feature

        RegisterPrivs("doors.all", DoorPrivs.All);
        RegisterPrivs("doors.servantLocked", DoorPrivs.ServantLocked);
        RegisterPrivs("doors.notServantLocked", DoorPrivs.NotServantLocked);
        RegisterPrivs("doors.thin", DoorPrivs.Thin);
        RegisterPrivs("doors.wide", DoorPrivs.Wide);
        RegisterPrivs("doors.fence", DoorPrivs.ThinFence);
        RegisterPrivs("doors.palisade", DoorPrivs.ThinPalisade);
        RegisterPrivs("doors.basic", DoorPrivs.ThinBasic);
        RegisterPrivs("doors.battlement", DoorPrivs.ThinBattlement);
        RegisterPrivs("doors.cordial", DoorPrivs.ThinCordial);
        RegisterPrivs("doors.verdant", DoorPrivs.ThinVerdant);
        RegisterPrivs("doors.royal", DoorPrivs.ThinRoyal);
        RegisterPrivs("doors.plagueSanctum", DoorPrivs.ThinPlagueSanctum);
        RegisterPrivs("doors.ancientSymphony1", DoorPrivs.ThinAncientSymphony1);
        RegisterPrivs("doors.ancientSymphony2", DoorPrivs.ThinAncientSymphony2);
        RegisterPrivs("doors.nocturnalOpulence", DoorPrivs.ThinNocturnalOpulence);
        RegisterPrivs("doors.dyedRed", DoorPrivs.DyedRed);
        RegisterPrivs("doors.dyedOrange", DoorPrivs.DyedOrange);
        RegisterPrivs("doors.dyedYellow", DoorPrivs.DyedYellow);
        RegisterPrivs("doors.dyedGreen", DoorPrivs.DyedGreen);
        RegisterPrivs("doors.dyedMintGreen", DoorPrivs.DyedMintGreen);
        RegisterPrivs("doors.dyedCyan", DoorPrivs.DyedCyan);
        RegisterPrivs("doors.dyedBlue", DoorPrivs.DyedBlue);
        RegisterPrivs("doors.dyedPurple", DoorPrivs.DyedPurple);
        RegisterPrivs("doors.dyedPink", DoorPrivs.DyedPink);
        RegisterPrivs("doors.dyedWhite", DoorPrivs.DyedWhite);
        RegisterPrivs("doors.dyedGrey", DoorPrivs.DyedGrey);
        RegisterPrivs("doors.dyedBlack", DoorPrivs.DyedBlack);
        RegisterPrivs("doors.prison", DoorPrivs.ThinPrison);
        RegisterPrivs("doors.barrier", DoorPrivs.ThinBarrier);
        RegisterPrivs("doors.wideBars", DoorPrivs.WideBars);
        RegisterPrivs("doors.widePlanks", DoorPrivs.WidePlanks);

        RegisterPrivs("sowSeed.all", SowSeedPrivs.All);

        RegisterPrivs("plantTree.all", PlantTreePrivs.All);
    }
    
    protected struct NamedCastlePrivileges
    {
        public string Prefix { get; set; }
        public string Name { get; set; }
        public CastlePrivileges Privs { get; set; }
    }

}
