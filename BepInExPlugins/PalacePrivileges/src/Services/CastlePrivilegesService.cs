using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using ProfuselyViolentProgression.Core.Utilities;
using ProfuselyViolentProgression.PalacePrivileges.Models;
using ProjectM;
using Unity.Entities;

namespace ProfuselyViolentProgression.PalacePrivileges.Services;

public class CastlePrivilegesService
{
    public static CastlePrivilegesService Instance;

    ManualLogSource _log;
    PlayerSettingsRepository _playerSettingsRepo;
    EntityManager _entityManager = WorldUtil.Server.EntityManager;

    CastlePrivileges _defaultClanPrivileges = new()
    {
        Misc = MiscPrivs.None,
        Build = BuildPrivs.None,
        Craft = CraftPrivs.AccessStations,
        Door = DoorPrivs.NotServantLocked | DoorPrivs.Wide,
        Prisoner = PrisonerPrivs.None,
        Servant = ServantPrivs.None,
        Teleporter = TeleporterPrivs.Waygate,
        Redistribution = RedistributionPrivs.None,
        Arena = ArenaPrivs.None,
        Research = ResearchPrivs.All,
    };

    public CastlePrivilegesService(
        ManualLogSource log,
        PlayerSettingsRepository playerSettingsRepo
    )
    {
        _log = log;
        _playerSettingsRepo = playerSettingsRepo;
    }

    public void LoadSettings()
    {
        _playerSettingsRepo.TryLoad();
    }

    public void SaveSettings()
    {
        _playerSettingsRepo.TrySave();
    }

    public PlayerSettings GetOrCreatePlayerSettings(ulong castleOwnerPlatformId)
    {
        if (!_playerSettingsRepo.TryGetPlayerSettings(castleOwnerPlatformId, out var playerSettings))
        {
            playerSettings = new PlayerSettings();
            playerSettings.ClanPrivs = _defaultClanPrivileges;
            _playerSettingsRepo.SetPlayerSettings(castleOwnerPlatformId, ref playerSettings);
            return playerSettings;
        }
        return playerSettings;
    }

    public CastlePrivileges PrivilegesForClan(ulong castleOwnerPlatformId)
    {
        var ownerSettings = GetOrCreatePlayerSettings(castleOwnerPlatformId);
        return ownerSettings.ClanPrivs;
    }

    public CastlePrivileges OverallPrivilegesForActingPlayerInClan(ulong castleOwnerPlatformId, ulong actingPlayerPlatformId)
    {
        var ownerSettings = GetOrCreatePlayerSettings(castleOwnerPlatformId);
        if (!ownerSettings.PlayerPrivsLookup.TryGetValue(actingPlayerPlatformId, out var actorPrivs))
        {
            return ownerSettings.ClanPrivs;
        }
        return (ownerSettings.ClanPrivs | actorPrivs.Granted) & ~actorPrivs.Forbidden;
    }

    public bool TryGetCustomPrivilegesForActingPlayer(ulong castleOwnerPlatformId, ulong actingPlayerPlatformId, out ActingPlayerPrivileges actingPlayerPrivileges)
    {
        var ownerSettings = GetOrCreatePlayerSettings(castleOwnerPlatformId);
        return ownerSettings.PlayerPrivsLookup.TryGetValue(actingPlayerPlatformId, out actingPlayerPrivileges);
    }

    public void ResetPlayerSettings(ulong castleOwnerPlatformId)
    {
        var newSettings = new PlayerSettings();
        newSettings.ClanPrivs = _defaultClanPrivileges;
        _playerSettingsRepo.SetPlayerSettings(castleOwnerPlatformId, ref newSettings);
    }

    public IEnumerable<string> NamesOfPlayersWithCustomPrivs(ulong castleOwnerPlatformId)
    {
        // todo: user lookup stuff could be optimized,
        // but keeping an up-to-date cache sounds like a lot of work.
        // Might see if I can yoink something from another mod.
        var userModelLookup = new Dictionary<ulong, UserUtil.UserModel>();
        foreach (var userModel in UserUtil.FindAllUsers())
        {
            userModelLookup[userModel.User.PlatformId] = userModel;
        }

        return PlatformIdsOfPlayersWithCustomPrivs(castleOwnerPlatformId)
            .Select(platformId => userModelLookup[platformId].User.CharacterName.ToString())
            .ToList();
    }

    public IEnumerable<ulong> PlatformIdsOfPlayersWithCustomPrivs(ulong castleOwnerPlatformId)
    {
        return GetOrCreatePlayerSettings(castleOwnerPlatformId)
            .PlayerPrivsLookup.Keys
            .ToList();
    }

    public void GrantClanPrivileges(ulong castleOwnerPlatformId, CastlePrivileges privs)
    {
        var settings = GetOrCreatePlayerSettings(castleOwnerPlatformId);
        settings.ClanPrivs |= privs;
        _playerSettingsRepo.SetPlayerSettings(castleOwnerPlatformId, ref settings);
    }

    public void UnGrantClanPrivileges(ulong castleOwnerPlatformId, CastlePrivileges privs)
    {
        var settings = GetOrCreatePlayerSettings(castleOwnerPlatformId);
        settings.ClanPrivs &= ~privs;
        _playerSettingsRepo.SetPlayerSettings(castleOwnerPlatformId, ref settings);
    }

    public void GrantPlayerPrivileges(ulong castleOwnerPlatformId, ulong targetPlayerPlatformId, CastlePrivileges privs)
    {
        var ownerSettings = GetOrCreatePlayerSettings(castleOwnerPlatformId);
        if (!ownerSettings.PlayerPrivsLookup.TryGetValue(targetPlayerPlatformId, out var actorPrivs))
        {
            actorPrivs = new ActingPlayerPrivileges();
        }
        actorPrivs.Granted |= privs;
        actorPrivs.Forbidden &= ~privs; // unforbid any granted privs
        ownerSettings.PlayerPrivsLookup[targetPlayerPlatformId] = actorPrivs;
        _playerSettingsRepo.SetPlayerSettings(castleOwnerPlatformId, ref ownerSettings);
    }

    public void UnGrantPlayerPrivileges(ulong castleOwnerPlatformId, ulong targetPlayerPlatformId, CastlePrivileges privs)
    {
        var ownerSettings = GetOrCreatePlayerSettings(castleOwnerPlatformId);
        if (!ownerSettings.PlayerPrivsLookup.TryGetValue(targetPlayerPlatformId, out var actorPrivs))
        {
            actorPrivs = new ActingPlayerPrivileges();
        }
        actorPrivs.Granted &= ~privs;
        ownerSettings.PlayerPrivsLookup[targetPlayerPlatformId] = actorPrivs;
        _playerSettingsRepo.SetPlayerSettings(castleOwnerPlatformId, ref ownerSettings);
    }

    public void ForbidPlayerPrivileges(ulong castleOwnerPlatformId, ulong targetPlayerPlatformId, CastlePrivileges privs)
    {
        var ownerSettings = GetOrCreatePlayerSettings(castleOwnerPlatformId);
        if (!ownerSettings.PlayerPrivsLookup.TryGetValue(targetPlayerPlatformId, out var actorPrivs))
        {
            actorPrivs = new ActingPlayerPrivileges();
        }
        actorPrivs.Forbidden |= privs;
        actorPrivs.Granted &= ~privs; // ungrant any forbidden privs
        ownerSettings.PlayerPrivsLookup[targetPlayerPlatformId] = actorPrivs;
        _playerSettingsRepo.SetPlayerSettings(castleOwnerPlatformId, ref ownerSettings);
    }

    public void UnForbidPlayerPrivileges(ulong castleOwnerPlatformId, ulong targetPlayerPlatformId, CastlePrivileges privs)
    {
        var ownerSettings = GetOrCreatePlayerSettings(castleOwnerPlatformId);
        if (!ownerSettings.PlayerPrivsLookup.TryGetValue(targetPlayerPlatformId, out var actorPrivs))
        {
            actorPrivs = new ActingPlayerPrivileges();
        }
        actorPrivs.Forbidden &= ~privs;
        ownerSettings.PlayerPrivsLookup[targetPlayerPlatformId] = actorPrivs;
        _playerSettingsRepo.SetPlayerSettings(castleOwnerPlatformId, ref ownerSettings);
    }

    // todo: can we do a wrapper thing to get rid of all this duplicated code

    public ulong GetPlatformIdOfTerritoryOwner(Entity castleTerritoryEntity)
    {
        // todo: check if this works
        return GetTerritoryOwnerRequestSystem.GetPlatformId(ref _entityManager, castleTerritoryEntity);
    }
    
}
