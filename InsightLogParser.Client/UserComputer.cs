using Microsoft.Win32;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace InsightLogParser.Client
{
    internal class UserComputer
    {
        private readonly Configuration _configuration;
        private readonly MessageWriter _messageWriter;

        public UserComputer(Configuration configuration, MessageWriter messageWriter)
        {
            _configuration = configuration;
            _messageWriter = messageWriter;
        }

        public string GetPrimaryLogFile()
        {
            if (Debugger.IsAttached) return DebugOverrides.LogFile;

            string logFolder;
            if (!string.IsNullOrWhiteSpace(_configuration.ForceLogFolder))
            {
                logFolder = _configuration.ForceLogFolder;
            }
            else
            {
                var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                logFolder = Path.Combine(appDataFolder, "IslandsofInsight", "Saved", "Logs");
            }

            const string logFileName = "IslandsofInsight.log";
            var logFile = Path.Combine(logFolder, logFileName);
            return logFile;
        }

        private string[]? FindSteamGameLibraryLocations()
        {
            //Find the steam install directory in the registry
            try
            {
                string? steamDirectory;
                using (RegistryKey? key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Valve\Steam"))
                {
                    if (key == null)
                    {
                        _messageWriter.WriteDebug("Registry: Steam registry key was null");
                        return null;
                    }
                    steamDirectory = key.GetValue("InstallPath") as string;
                    if (steamDirectory == null)
                    {
                        _messageWriter.WriteDebug("Registry: Steam InstallPath was null");
                        return null;
                    }
                }

                var steamConfig = Path.Combine(steamDirectory, "config", "config.vdf");
                if (!File.Exists(steamConfig))
                {
                    _messageWriter.WriteDebug($"Steam config file '{steamConfig}' does not exist");
                    return null;
                }

                //Crude, but it works (on my machine)
                var config = File.ReadAllText(steamConfig);
                var steamInstallFolders = Regex.Matches(config, @"\""BaseInstallFolder_\d+\""\s*\""(.*?)\""")
                    .Where(m => m.Success)
                    .Select(x => x.Groups[1].Value.Replace(@"\\", @"\"))
                    .ToList();
                _messageWriter.WriteDebug($"Found {steamInstallFolders.Count} steam installation locations");
                return steamInstallFolders.ToArray();
            }
            catch (Exception ex)
            {
                _messageWriter.WriteDebug($"Exception when opening registry: {ex}");
                return null;
            }
        }

        public string? GetPuzzleJsonPath()
        {
            if (Debugger.IsAttached) return DebugOverrides.JsonFile;

            var puzzleJsonPath = Path.Combine("IslandsofInsight", "Content", "ASophia", "PuzzleDatabase", "Puzzles.json");

            if (!string.IsNullOrWhiteSpace(_configuration.ForceGameRootFolder))
            {
                var gameFolder = _configuration.ForceGameRootFolder;
                var jsonPath = Path.Combine(gameFolder, puzzleJsonPath);
                return jsonPath;
            }

            var steamGameFolders = FindSteamGameLibraryLocations();
            if (steamGameFolders == null)
            {
                _messageWriter.WriteError("Failed to find any steam library locations");
                return null;
            }

            foreach (var steamGameFolder in steamGameFolders)
            {
                var subPath = Path.Combine("steamapps", "common", "Islands of Insight", "IslandsofInsight", "Content", "ASophia", "PuzzleDatabase", "Puzzles.json");
                var jsonPath = Path.Combine(steamGameFolder, subPath);
                _messageWriter.WriteDebug($"Checking: '{jsonPath}'");
                if (!File.Exists(jsonPath)) continue;
                return jsonPath;
            }

            return null;
        }

        public bool IsGameRunning()
        {
            if (Debugger.IsAttached) return DebugOverrides.IsGameRunning;

            var islandProcesses = Process.GetProcessesByName("IslandsofInsight");
            var running = islandProcesses.Length > 0;
            foreach (var process in islandProcesses)
            {
                process.Dispose();;
            }
            return running;
        }
    }
}
