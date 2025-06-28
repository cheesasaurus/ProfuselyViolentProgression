using System;
using System.IO;
using BepInEx.Configuration;

namespace ProfuselyViolentProgression.Core.Config;

public class BepInExConfigReloader
{
    private ConfigFile _config;

    private string _filename;
    private string _absoluteFilePath;
    private string _absoluteFileDir;

    private FileSystemWatcher _fileWatcher;


    public BepInExConfigReloader(ConfigFile config)
    {
        _config = config;
        _absoluteFilePath = config.ConfigFilePath;
        _absoluteFileDir = Path.GetDirectoryName(_absoluteFilePath);
        _filename = Path.GetFileName(_absoluteFilePath);
        InitFileWatcher();
    }

    private void InitFileWatcher()
    {
        _fileWatcher = new FileSystemWatcher(_absoluteFileDir);
        _fileWatcher.Filter = _filename;
        _fileWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime;
        _fileWatcher.Changed += HandleConfigFileChanged;
        _fileWatcher.EnableRaisingEvents = true;
    }

    private void HandleConfigFileChanged(object sender, FileSystemEventArgs e)
    {
        _config.Reload();
    }

    public void Dispose()
    {
        _fileWatcher.Changed -= HandleConfigFileChanged;
    }

}