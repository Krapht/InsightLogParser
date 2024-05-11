using System.Text;
using System.Text.Json;

namespace InsightLogParser.Client;

internal class ConfigurationManager
{
    private readonly string _configurationFilename;
    private readonly MessageWriter _messageWriter;
    private readonly JsonSerializerOptions _serializerOptions;

    public ConfigurationManager(string configurationFilename, MessageWriter messageWriter)
    {
        _configurationFilename = configurationFilename;
        _messageWriter = messageWriter;

        _serializerOptions = new JsonSerializerOptions()
        {
            WriteIndented = true,
        };
    }

    private async Task<Configuration> SaveNewConfigurationAsync()
    {
        var newConfig = new Configuration();
        await SaveConfigurationAsync(newConfig).ConfigureAwait(ConfigureAwaitOptions.None);
        return newConfig;
    }


    public async Task<Configuration> LoadConfigurationAsync()
    {
        if (!File.Exists(_configurationFilename))
        {
            _messageWriter.WriteInitLine("Creating new config file...", ConsoleColor.Green);
            return await SaveNewConfigurationAsync().ConfigureAwait(ConfigureAwaitOptions.None);
        }

        var jsonText = await File.ReadAllTextAsync(_configurationFilename, Encoding.UTF8).ConfigureAwait(ConfigureAwaitOptions.None);
        try
        {
            var loadedConfig = JsonSerializer.Deserialize<Configuration>(jsonText);
            if (loadedConfig == null)
            {
                _messageWriter.WriteInitLine("Failed to load configuration, replacing with new config...", ConsoleColor.Red);
                return await SaveNewConfigurationAsync().ConfigureAwait(ConfigureAwaitOptions.None);
            }
            _messageWriter.WriteInitLine($"Loaded configuration from '{_configurationFilename}'", ConsoleColor.Green);
            if (loadedConfig.ConfigVersion < Configuration.CurrentConfigurationVersion)
            {
                loadedConfig.ConfigVersion = Configuration.CurrentConfigurationVersion;
                _messageWriter.WriteInitLine("Updating configuration file to new version...", ConsoleColor.Yellow);
                await SaveConfigurationAsync(loadedConfig).ConfigureAwait(ConfigureAwaitOptions.None);
            }
            return loadedConfig;
        }
        catch (Exception)
        {
            _messageWriter.WriteInitLine("Failed to load configuration, replacing with new config...", ConsoleColor.Red);
            return await SaveNewConfigurationAsync().ConfigureAwait(ConfigureAwaitOptions.None);
        }
    }

    private async Task SaveConfigurationAsync(Configuration config)
    {
        var jsonText = JsonSerializer.Serialize(config, _serializerOptions);
        await File.WriteAllTextAsync(_configurationFilename, jsonText, Encoding.UTF8).ConfigureAwait(ConfigureAwaitOptions.None);
        _messageWriter.WriteInfo($"Saved configuration to '{_configurationFilename}'");
    }

    public void ValidateConfiguration(Configuration config)
    {
        var allOk = true;

        allOk &= ValidateBeepFrequency(nameof(Configuration.NewParseBeepFrequency), config.NewParseBeepFrequency);
        allOk &= ValidateBeepDuration(nameof(Configuration.NewParseBeepDuration), config.NewParseBeepDuration);

        allOk &= ValidateBeepFrequency(nameof(Configuration.KnownParseBeepFrequency), config.KnownParseBeepFrequency);
        allOk &= ValidateBeepDuration(nameof(Configuration.KnownParseBeepDuration), config.KnownParseBeepDuration);

        allOk &= ValidateBeepFrequency(nameof(Configuration.OpenSolvedPuzzleBeepFrequency), config.OpenSolvedPuzzleBeepFrequency);
        allOk &= ValidateBeepDuration(nameof(Configuration.OpenSolvedPuzzleBeepDuration), config.OpenSolvedPuzzleBeepDuration);

        allOk &= ValidateBeepFrequency(nameof(Configuration.BeepForAttentionFrequency), config.BeepForAttentionFrequency);
        allOk &= ValidateBeepDuration(nameof(Configuration.BeepForAttentionDuration), config.BeepForAttentionDuration);
        if (config.BeepForAttentionInterval < Beeper.MinDelay || config.BeepForAttentionInterval > Beeper.MaxDelay)
        {
            _messageWriter.WriteInitLine($"Configuration invalid: {nameof(Configuration.BeepForAttentionInterval)} must be between {Beeper.MinDelay} and {Beeper.MaxDelay} (inclusive)", ConsoleColor.Red);
            allOk = false;
        }

        if (config.BeepForAttentionCount < Beeper.MinBeeps || config.BeepForAttentionCount > Beeper.MaxBeeps)
        {
            _messageWriter.WriteInitLine($"Configuration invalid: {nameof(Configuration.BeepForAttentionCount)} must be between {Beeper.MinBeeps} and {Beeper.MaxBeeps} (inclusive)", ConsoleColor.Red);
            allOk = false;
        }

        if (allOk)
        {
            _messageWriter.WriteInitLine($"Configuration validated ok", ConsoleColor.Green);
        }
    }

    private bool ValidateBeepFrequency(string configName, int value)
    {
        if (value < Beeper.MinFrequency || value > Beeper.MaxFrequency)
        {
            _messageWriter.WriteInitLine($"Configuration invalid: {configName} must be between {Beeper.MinFrequency} and {Beeper.MaxFrequency} (inclusive)", ConsoleColor.Red);
            return false;
        }
        return true;
    }

    private bool ValidateBeepDuration(string configName, int value)
    {
        if (value < Beeper.MinDuration || value > Beeper.MaxDuration)
        {
            _messageWriter.WriteInitLine($"Configuration invalid: {configName} must be between {Beeper.MinDuration} and {Beeper.MaxDuration} (inclusive)", ConsoleColor.Red);
            return false;
        }
        return true;
    }

}