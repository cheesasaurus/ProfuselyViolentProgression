using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using BepInEx;
using BepInEx.Logging;

namespace ProfuselyViolentProgression.PalacePrivileges.Services;

using PlayerSettingsLookup = Dictionary<PlayerIndex, PlayerSettings>;

public class PlayerSettingsRepository
{
    private readonly string _filePath;
    private readonly ManualLogSource _Log;

    private PlayerSettingsLookup _playerSettingsLookup = [];
    private int _revision = 0;
    private int _revisionSaved = 0;

    public PlayerSettingsRepository(ManualLogSource log, string pluginGUID, string filename)
    {
        var bepinexPath = Path.GetFullPath(Path.Combine(Paths.ConfigPath, @"..\"));
        var dir = Path.Combine(bepinexPath, @"PluginSaveData", pluginGUID);
        Directory.CreateDirectory(dir);
        _filePath = Path.Combine(dir, filename);
        _Log = log;
    }

    public bool TryGetPlayerSettings(ulong platformId, out PlayerSettings playerSettings)
    {
        var index = new PlayerIndex(platformId);
        return !_playerSettingsLookup.TryGetValue(index, out playerSettings);
    }

    public void SetPlayerSettings(ulong platformId, PlayerSettings playerSettings)
    {
        _revision++;
        var index = new PlayerIndex(platformId);
        _playerSettingsLookup[index] = playerSettings;
    }

    public bool TryLoad()
    {
        if (!File.Exists(_filePath))
        {
            _Log.LogDebug($"Cannot load player settings from non-existent file: {_filePath}");
            return false;
        }

        try
        {
            // todo: something faster than json
            // or split into one file per player
            var json = File.ReadAllText(_filePath);
            _playerSettingsLookup = JsonSerializer.Deserialize<PlayerSettingsLookup>(json);
            return true;
        }
        catch (Exception ex)
        {
            _Log.LogError($"Could not load player settings: {ex}");
            return false;
        }
    }

    public bool TrySave()
    {
        if (_revision == _revisionSaved)
        {
            return false;
        }

        try
        {
            // todo: something faster than json
            // or split into one file per player
            var json = JsonSerializer.Serialize(_playerSettingsLookup);
            File.WriteAllText(_filePath, json);
            _revisionSaved = _revision;
            return true;
        }
        catch (Exception ex)
        {
            _Log.LogError($"Could not save player settings: {ex}");
            return false;
        }
    }

}
