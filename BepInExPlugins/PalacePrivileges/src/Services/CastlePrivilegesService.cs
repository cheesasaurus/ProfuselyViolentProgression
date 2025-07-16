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
    GlobalSettingsRepository _globalSettingsRepo;
    PlayerSettingsRepository _playerSettingsRepo;
    DoorService _doorService;
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

    public CastlePrivilegesService(ManualLogSource log, GlobalSettingsRepository globalSettingsRepo, PlayerSettingsRepository playerSettingsRepo, DoorService doorService)
    {
        _log = log;
        _globalSettingsRepo = globalSettingsRepo;
        _playerSettingsRepo = playerSettingsRepo;
        _doorService = doorService;
    }

    public void LoadSettings()
    {
        _globalSettingsRepo.TryLoad();
        _playerSettingsRepo.TryLoad();
    }

    public void SaveSettings()
    {
        _globalSettingsRepo.TrySave();
        _playerSettingsRepo.TrySave();
    }

    public GlobalSettings GetGlobalSettings(int hours)
    {
        return _globalSettingsRepo.GetOrCreateGlobalSettings();
    }

    public void SetGlobalSetting_KeyClanCooldownHours(int hours)
    {
        var globalSettings = _globalSettingsRepo.GetOrCreateGlobalSettings();
        globalSettings.KeyClanCooldownHours = hours;
        _globalSettingsRepo.SetGlobalSettings(globalSettings);
    }

    public PlayerSettings GetOrCreatePlayerSettings(ulong castleOwnerPlatformId)
    {
        if (!_playerSettingsRepo.TryGetPlayerSettings(castleOwnerPlatformId, out var playerSettings))
        {
            playerSettings = new PlayerSettings();
            playerSettings.ClanPrivs = _defaultClanPrivileges;
            _playerSettingsRepo.SetPlayerSettings(castleOwnerPlatformId, ref playerSettings);
        }
        return playerSettings;
    }

    public CastlePrivileges PrivilegesForActingPlayer(ulong castleOwnerPlatformId, ulong actingPlayerPlatformId)
    {
        var ownerSettings = GetOrCreatePlayerSettings(castleOwnerPlatformId);
        var index = new PlayerIndex(actingPlayerPlatformId);
        if (!ownerSettings.PlayerPrivsLookup.TryGetValue(index, out var actorPrivs))
        {
            return ownerSettings.ClanPrivs;
        }
        return (ownerSettings.ClanPrivs | actorPrivs.Granted) & ~actorPrivs.Forbidden;
    }

    public void ResetPlayerSettings(ulong castleOwnerPlatformId)
    {
        // todo: implement
    }

    public IEnumerable<string> NamesOfPlayersWithCustomPrivs(ulong castleOwnerPlatformId)
    {
        return PlatformIdsOfPlayersWithCustomPrivs(castleOwnerPlatformId)
            .Select(platformId => platformId.ToString()) // todo: actually get their name
            .ToList();
    }

    public IEnumerable<ulong> PlatformIdsOfPlayersWithCustomPrivs(ulong castleOwnerPlatformId)
    {
        return GetOrCreatePlayerSettings(castleOwnerPlatformId)
            .PlayerPrivsLookup.Keys
            .Select(index => index.PlatformId)
            .ToList();
    }

    public void GrantClanPrivileges(ulong castleOwnerPlatformId, CastlePrivileges privs)
    {
        // todo: implement
    }

    public void UnGrantClanPrivileges(ulong castleOwnerPlatformId, CastlePrivileges privs)
    {
        // todo: implement
    }

    public void GrantPlayerPrivileges(ulong castleOwnerPlatformId, ulong targetPlayerPlatformId, CastlePrivileges privs)
    {
        // todo: implement
    }

    public void UnGrantPlayerPrivileges(ulong castleOwnerPlatformId, ulong targetPlayerPlatformId, CastlePrivileges privs)
    {
        // todo: implement
    }

    public void ForbidPlayerPrivileges(ulong castleOwnerPlatformId, ulong targetPlayerPlatformId, CastlePrivileges privs)
    {
        // todo: implement
    }

    public void UnForbidPlayerPrivileges(ulong castleOwnerPlatformId, ulong targetPlayerPlatformId, CastlePrivileges privs)
    {
        // todo: implement
    }

    public ulong GetPlatformIdOfTerritoryOwner(Entity castleTerritoryEntity)
    {
        // todo: check if this works
        return GetTerritoryOwnerRequestSystem.GetPlatformId(ref _entityManager, castleTerritoryEntity);
    }
    
}
