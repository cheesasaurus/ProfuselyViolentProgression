using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using BepInEx;
using BepInEx.Logging;

namespace ProfuselyViolentProgression.PalacePrivileges.Services;

using PlayerSettingsLookup = Dictionary<PlayerIndex, PlayerSettings>;

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
        var index = new PlayerIndex(platformId);
        return !_playerSettingsLookup.TryGetValue(index, out playerSettings);
    }

    public void SetPlayerSettings(ulong platformId, ref PlayerSettings playerSettings)
    {
        playerSettings.Revision++;
        var index = new PlayerIndex(platformId);
        _playerSettingsLookup[index] = playerSettings;
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
            var keys = _playerSettingsLookup.Keys;
            foreach (var index in keys)
            {
                SaveSettings_ForOnePlayer(index);
            }
            if (keys.Any())
            {
                _log.LogDebug($"Saved updated player settings for {keys.Count} players.");
            }
            return true;
        }
        catch (Exception ex)
        {
            _log.LogError($"Could not save player settings: {ex}");
            return false;
        }
    }

    private void LoadSettings_ForOnePlayer(string filePath)
    {
        var json = File.ReadAllText(_dirPath);
        var playerSettings = JsonSerializer.Deserialize<PlayerSettings>(json);
        playerSettings.RevisionSaved = playerSettings.Revision;

        var platformId = ulong.Parse(Path.GetFileNameWithoutExtension(filePath));
        var index = new PlayerIndex(platformId);
        _playerSettingsLookup[index] = playerSettings;
    }

    private void SaveSettings_ForOnePlayer(PlayerIndex index)
    {
        var playerSettings = _playerSettingsLookup[index];
        if (playerSettings.Revision == playerSettings.RevisionSaved)
        {
            return;
        }
        var json = JsonSerializer.Serialize(playerSettings);
        File.WriteAllText($"{_dirPath}/{index.PlatformId}.json", json);
        playerSettings.RevisionSaved = playerSettings.Revision;
        _playerSettingsLookup[index] = playerSettings;
    }

}
