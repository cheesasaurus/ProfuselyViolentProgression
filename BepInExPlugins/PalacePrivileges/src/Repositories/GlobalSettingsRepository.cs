using System;
using System.IO;
using System.Text.Json;
using BepInEx;
using BepInEx.Logging;
using ProfuselyViolentProgression.PalacePrivileges.Models;

namespace ProfuselyViolentProgression.PalacePrivileges.Services;

public class GlobalSettingsRepository
{
    private readonly string _filePath;
    private readonly ManualLogSource _log;

    private GlobalSettings _globalSettings;
    private int _revision = 0;
    private int _revisionSaved = 0;

    public GlobalSettingsRepository(ManualLogSource log, string pluginGUID, string filename)
    {
        var bepinexPath = Path.GetFullPath(Path.Combine(Paths.ConfigPath, @"..\"));
        var dir = Path.Combine(bepinexPath, @"PluginSaveData", pluginGUID);
        Directory.CreateDirectory(dir);
        _filePath = Path.Combine(dir, filename);
        _log = log;
    }

    public GlobalSettings GetOrCreateGlobalSettings()
    {
        return _globalSettings;
    }

    public void SetGlobalSettings(GlobalSettings settings)
    {
        _revision++;
        _globalSettings = settings;
    }

    public bool TryLoad()
    {
        if (!File.Exists(_filePath))
        {
            _log.LogDebug($"Cannot load global settings from non-existent file: {_filePath}");
            return false;
        }

        try
        {
            var json = File.ReadAllText(_filePath);
            _globalSettings = JsonSerializer.Deserialize<GlobalSettings>(json);
            return true;
        }
        catch (Exception ex)
        {
            _log.LogError($"Could not load global settings: {ex}");
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
            var json = JsonSerializer.Serialize(_globalSettings);
            File.WriteAllText(_filePath, json);
            _revisionSaved = _revision;
            _log.LogDebug("Saved global settings.");
            return true;
        }
        catch (Exception ex)
        {
            _log.LogError($"Could not save global settings: {ex}");
            return false;
        }
    }

}
