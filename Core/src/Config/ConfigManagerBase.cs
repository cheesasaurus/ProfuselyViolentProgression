using System;
using System.IO;
using System.Reflection;
using ProfuselyViolentProgression.Core.Utilities;

namespace ProfuselyViolentProgression.Core.Config;

public abstract class ConfigManagerBase<TConfig> : IConfigManager<TConfig>
{
    private string _filename;
    public string FileName => _filename;

    private string _absoluteFilePath;
    public string AbsoluteFilePath => _absoluteFilePath;

    private string _absoluteFileDir;
    public string AbsoluteFileDir => _absoluteFileDir;

    public event Action<TConfig> ConfigUpdated;

    private FileSystemWatcher _fileWatcher;

    public ConfigManagerBase(string pluginGUID, string filename)
    {
        _filename = filename;
        _absoluteFileDir = Path.Combine(BepInEx.Paths.ConfigPath, pluginGUID);
        Directory.CreateDirectory(_absoluteFileDir);
        _absoluteFilePath = Path.Combine(_absoluteFileDir, filename);
        InitFileWatcher();
    }

    private void InitFileWatcher()
    {
        _fileWatcher = new FileSystemWatcher(AbsoluteFileDir);
        _fileWatcher.Filter = _filename;
        _fileWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime;
        _fileWatcher.Changed += HandleConfigFileChanged;
        _fileWatcher.EnableRaisingEvents = true;
    }

    private void HandleConfigFileChanged(object sender, FileSystemEventArgs e)
    {
        if (TryLoadConfig(out var config))
        {
            ConfigUpdated?.Invoke(config);
        }
    }

    public void Dispose()
    {
        _fileWatcher.Changed -= HandleConfigFileChanged;
    }

    public void CreateMainFile_FromResource(string resourceName, bool overwrite = false)
    {
        CreateExampleFile_FromResource(resourceName, _filename, overwrite);
    }

    public void CreateExampleFile_FromResource(string resourceName, string toFilename, bool overwrite = false)
    {
        var outputFilepath = Path.Combine(AbsoluteFileDir, toFilename);
        if (!overwrite && File.Exists(outputFilepath))
        {
            return;
        }

        var assembly = Assembly.GetExecutingAssembly();
        using (Stream inputStream = assembly.GetManifestResourceStream(resourceName))
        {
            using (FileStream outputStream = File.Create(outputFilepath))
            {
                inputStream.CopyTo(outputStream);
            }
        }
    }

    abstract public bool TryLoadConfig(out TConfig config);
}
