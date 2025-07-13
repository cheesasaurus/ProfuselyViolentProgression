using System;
using System.Collections.Generic;
using ProfuselyViolentProgression.PalacePrivileges.Models;

namespace ProfuselyViolentProgression.LoadoutLockdown.Commands;

public class PrivilegeParser
{
    public static PrivilegeParser Instance;

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

    public Dictionary<string, List<string>> PrivilegeNamesGrouped(CastlePrivileges privs)
    {
        var groupedNames = new Dictionary<string, List<string>>();
        groupedNames.Add("", []);

        foreach (var (key, namedPriv) in PrivsLookup)
        {
            if (privs.IsSuperset(namedPriv.Privs))
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

    private void RegisterPrivs(string name, PrisonerPrivs privs)
    {
        RegisterPrivs(name, new CastlePrivileges
        {
            Prisoner = privs,
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

    private void InitPrivsLookup()
    {
        // todo: comment out anything not functional before release

        RegisterPrivs("all", CastlePrivileges.All);

        RegisterPrivs("lockbox", MiscPrivs.Lockbox);
        RegisterPrivs("musicbox", MiscPrivs.Musicbox); // planned feature
        RegisterPrivs("throne", MiscPrivs.Throne); // planned feature
        RegisterPrivs("research", MiscPrivs.Research); // planned feature
        RegisterPrivs("plantSeeds", MiscPrivs.PlantSeeds); // planned feature

        RegisterPrivs("build.all", BuildPrivs.All);

        RegisterPrivs("craft.all", CraftPrivs.All);
        RegisterPrivs("craft.useTreasury", CraftPrivs.UseTreasury);
        RegisterPrivs("craft.accessStations", CraftPrivs.AccessStations); // planned feature
        RegisterPrivs("craft.toggleRecipes", CraftPrivs.ToggleRecipes); // planned feature

        RegisterPrivs("prisoners.all", PrisonerPrivs.All);  // planned feature
        RegisterPrivs("prisoners.subdue", PrisonerPrivs.Subdue);  // planned feature
        RegisterPrivs("prisoners.kill", PrisonerPrivs.Kill);  // planned feature
        RegisterPrivs("prisoners.extractBlood", PrisonerPrivs.ExtractBlood); // planned feature
        RegisterPrivs("prisoners.feedSafeFood", PrisonerPrivs.FeedSafeFood); // planned feature
        RegisterPrivs("prisoners.feedUnSafeFood", PrisonerPrivs.FeedUnSafeFood); // planned feature

        RegisterPrivs("servants.all", ServantPrivs.All); // planned feature
        RegisterPrivs("servants.convert", ServantPrivs.Convert); // planned feature
        RegisterPrivs("servants.terminate", ServantPrivs.Terminate); // planned feature
        RegisterPrivs("servants.gear", ServantPrivs.Gear); // planned feature
        RegisterPrivs("servants.rename", ServantPrivs.Rename); // planned feature

        RegisterPrivs("tp.all", TeleporterPrivs.All); // planned feature
        RegisterPrivs("tp.waygate", TeleporterPrivs.Waygate); // planned feature
        RegisterPrivs("tp.waygateOut", TeleporterPrivs.WaygateOut); // planned feature
        RegisterPrivs("tp.waygateIn", TeleporterPrivs.WaygateIn); // planned feature
        RegisterPrivs("tp.red", TeleporterPrivs.Red); // planned feature
        RegisterPrivs("tp.yellow", TeleporterPrivs.Yellow); // planned feature
        RegisterPrivs("tp.purple", TeleporterPrivs.Purple); // planned feature
        RegisterPrivs("tp.blue", TeleporterPrivs.Blue); // planned feature
        RegisterPrivs("tp.allSmall", TeleporterPrivs.AllSmall); // planned feature

        RegisterPrivs("redist.all", RedistributionPrivs.All); // planned feature
        RegisterPrivs("redist.quickSend", RedistributionPrivs.QuickSend); // planned feature
        RegisterPrivs("redist.toggleAutoSend", RedistributionPrivs.ToggleAutoSend); // planned feature
        RegisterPrivs("redist.edit", RedistributionPrivs.Edit); // planned feature

        RegisterPrivs("arena.all", ArenaPrivs.All); // planned feature
        RegisterPrivs("arena.startContest", ArenaPrivs.StartContest); // planned feature
        RegisterPrivs("arena.editRules", ArenaPrivs.EditRules); // planned feature
        RegisterPrivs("arena.zonePainting", ArenaPrivs.ZonePainting); // planned feature

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
        RegisterPrivs("doors.noctOp", DoorPrivs.ThinNocturnalOpulence);
        RegisterPrivs("doors.noctOpRed", DoorPrivs.ThinNocturnalOpulenceRed); // planned feature
        RegisterPrivs("doors.noctOpOrange", DoorPrivs.ThinNocturnalOpulenceOrange); // planned feature
        RegisterPrivs("doors.noctOpYellow", DoorPrivs.ThinNocturnalOpulenceYellow); // planned feature
        RegisterPrivs("doors.noctOpGreen", DoorPrivs.ThinNocturnalOpulenceGreen); // planned feature
        RegisterPrivs("doors.noctOpGreenLight", DoorPrivs.ThinNocturnalOpulenceGreenLight); // planned feature
        RegisterPrivs("doors.noctOpBlueLight", DoorPrivs.ThinNocturnalOpulenceBlueLight); // planned feature
        RegisterPrivs("doors.noctOpBlue", DoorPrivs.ThinNocturnalOpulenceBlue); // planned feature
        RegisterPrivs("doors.noctOpPurple", DoorPrivs.ThinNocturnalOpulencePurple); // planned feature
        RegisterPrivs("doors.noctOpPink", DoorPrivs.ThinNocturnalOpulencePink); // planned feature
        RegisterPrivs("doors.noctOpWhite", DoorPrivs.ThinNocturnalOpulenceWhite); // planned feature
        RegisterPrivs("doors.noctOpGrey", DoorPrivs.ThinNocturnalOpulenceGrey); // planned feature
        RegisterPrivs("doors.noctOpBlack", DoorPrivs.ThinNocturnalOpulenceBlack); // planned feature
        RegisterPrivs("doors.prison", DoorPrivs.ThinPrison);
        RegisterPrivs("doors.barrier", DoorPrivs.ThinBarrier);
        RegisterPrivs("doors.wideBars", DoorPrivs.WideBars);
        RegisterPrivs("doors.widePlanks", DoorPrivs.WidePlanks);
    }
    
    protected struct NamedCastlePrivileges
    {
        public string Prefix { get; set; }
        public string Name { get; set; }
        public CastlePrivileges Privs { get; set; }
    }

}
