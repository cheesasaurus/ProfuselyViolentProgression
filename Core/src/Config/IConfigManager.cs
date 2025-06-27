using System;

namespace ProfuselyViolentProgression.Core.Config;


/// <summary>
/// An instance of <c>IConfigManager</c> is responsible for managing a single configuration file.
/// Creating the file to be edited. Parsing the file. Watching for changes to the file.
/// </summary>
public interface IConfigManager<TConfig>
{
    public string FileName { get; }
    public string AbsoluteFilePath { get; }
    public string AbsoluteFileDir { get; }

    public event Action<TConfig> ConfigUpdated;

    public bool TryLoadConfig(out TConfig config);

    public void CreateMainFile_FromResource(string resourceName, bool overwrite = false);
    public void CreateExampleFile_FromResource(string resourceName, string toFilename, bool overwrite = false);

    public void Dispose();
}