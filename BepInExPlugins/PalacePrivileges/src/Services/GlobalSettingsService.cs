using System;
using BepInEx.Logging;
using ProfuselyViolentProgression.PalacePrivileges.Models;

namespace ProfuselyViolentProgression.PalacePrivileges.Services;

public class GlobalSettingsService
{
    public event Action<GlobalSettings> GlobalSettingsChanged;
    private ManualLogSource _log;
    private GlobalSettingsRepository _globalSettingsRepo;

    public GlobalSettingsService(
        ManualLogSource log,
        GlobalSettingsRepository globalSettingsRepo
    )
    {
        _log = log;
        _globalSettingsRepo = globalSettingsRepo;
    }

    public void LoadSettings()
    {
        _globalSettingsRepo.TryLoad();
    }

    public void SaveSettings()
    {
        _globalSettingsRepo.TrySave();
    }

    public GlobalSettings GetGlobalSettings()
    {
        return _globalSettingsRepo.GetOrCreateGlobalSettings();
    }

    public void SetGlobalSetting_KeyClanCooldownHours(float hours)
    {
        var globalSettings = _globalSettingsRepo.GetOrCreateGlobalSettings();
        globalSettings.KeyClanCooldownHours = hours;
        _globalSettingsRepo.SetGlobalSettings(ref globalSettings);
        OnGlobalSettingsChanged(globalSettings);
    }

    public void SetGlobalSetting_DebugLogRulings(bool enabled)
    {
        var globalSettings = _globalSettingsRepo.GetOrCreateGlobalSettings();
        globalSettings.DebugLogRulings = enabled;
        _globalSettingsRepo.SetGlobalSettings(ref globalSettings);
        OnGlobalSettingsChanged(globalSettings);
    }

    // todo: some kind of wrapper thing to reduce code duplication
    
    private void OnGlobalSettingsChanged(GlobalSettings newSettings)
    {
        GlobalSettingsChanged?.Invoke(newSettings);
    }

}