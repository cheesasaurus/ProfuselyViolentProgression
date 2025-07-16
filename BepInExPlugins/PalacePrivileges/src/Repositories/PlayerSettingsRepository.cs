using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using BepInEx;
using BepInEx.Logging;

namespace ProfuselyViolentProgression.PalacePrivileges.Services;

using PlayerSettingsLookup = Dictionary<ulong, PlayerSettings>;

/// <summary>
/// <para>The PlayerSettingsRepository is responsible for saving/loading player settings.</para>
/// 
/// There are some optimizations:
/// <list type="bullet">
///   <item>Settings are saved and loaded on a per-player basis.</item>
///   <item>
///     Only revisions are saved.
///     Any settings that haven't changed since the last save, will not be written to disk.
///   </item>
///   <item>
///     (Players who never edited their settings, will therefore not have any save files.)
///   </item>
/// </list>
/// </summary>
public class PlayerSettingsRepository
{
    private readonly string _dirPath;
    private readonly ManualLogSource _log;

    private readonly PlayerSettingsLookup _playerSettingsLookup = [];

    public PlayerSettingsRepository(ManualLogSource log, string pluginGUID, string dirName)
    {
        var bepinexPath = Path.GetFullPath(Path.Combine(Paths.ConfigPath, @"..\"));
        var saveDataDir = Path.Combine(bepinexPath, @"PluginSaveData", pluginGUID);
        _dirPath = Path.Combine(saveDataDir, dirName);
        Directory.CreateDirectory(_dirPath);
        _log = log;

    }

    public bool TryGetPlayerSettings(ulong platformId, out PlayerSettings playerSettings)
    {
        return _playerSettingsLookup.TryGetValue(platformId, out playerSettings);
    }

    public void SetPlayerSettings(ulong platformId, ref PlayerSettings playerSettings)
    {
        playerSettings.Revision++;
        _playerSettingsLookup[platformId] = playerSettings;
    }

    public bool TryLoad()
    {
        try
        {
            var filePaths = Directory.GetFiles(_dirPath, "*.json");
            foreach (var filePath in filePaths)
            {
                LoadSettings_ForOnePlayer(filePath);
            }
            return true;
        }
        catch (Exception ex)
        {
            _log.LogError($"Could not load player settings: {ex}");
            return false;
        }
    }

    public bool TrySave()
    {
        try
        {
            var saveCount = 0;
            foreach (var platformId in _playerSettingsLookup.Keys)
            {
                var saved = MaybeSaveSettings_ForOnePlayer(platformId);
                if (saved)
                {
                    saveCount++;
                }
            }
            if (saveCount > 0)
            {
                _log.LogDebug($"Saved updated player settings for {saveCount} players.");
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            _log.LogError($"Could not save player settings: {ex}");
            return false;
        }
    }

    private void LoadSettings_ForOnePlayer(string filePath)
    {
        var json = File.ReadAllText(filePath);
        var playerSettings = JsonSerializer.Deserialize<PlayerSettings>(json);
        playerSettings.RevisionSaved = playerSettings.Revision;

        var platformId = ulong.Parse(Path.GetFileNameWithoutExtension(filePath));
        _playerSettingsLookup[platformId] = playerSettings;
    }

    private bool MaybeSaveSettings_ForOnePlayer(ulong platformId)
    {
        var playerSettings = _playerSettingsLookup[platformId];
        if (playerSettings.Revision == playerSettings.RevisionSaved)
        {
            return false;
        }
        var json = JsonSerializer.Serialize(playerSettings);
        File.WriteAllText($"{_dirPath}/{platformId}.json", json);
        playerSettings.RevisionSaved = playerSettings.Revision;
        _playerSettingsLookup[platformId] = playerSettings;
        return true;
    }

}
