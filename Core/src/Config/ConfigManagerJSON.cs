using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using BepInEx.Logging;

namespace ProfuselyViolentProgression.Core.Config;

public class ConfigManagerJSON<TConfig> : ConfigManagerBase<TConfig>
{
    private ManualLogSource _log;
    private JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
    {
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase),
        },
    };

    public ConfigManagerJSON(string pluginGUID, string filename, ManualLogSource log) : base(pluginGUID, filename)
    {
        _log = log;
    }

    public override bool TryLoadConfig([NotNullWhen(true)] out TConfig config)
    {
        try
        {
            var json = File.ReadAllText(AbsoluteFilePath);
            config = JsonSerializer.Deserialize<TConfig>(json, _jsonSerializerOptions);
            return true;
        }
        catch (Exception ex)
        {
            config = default;
            _log.LogError($"Error parsing {AbsoluteFilePath}: {ex}");
        }
        return false;
    }
    
}