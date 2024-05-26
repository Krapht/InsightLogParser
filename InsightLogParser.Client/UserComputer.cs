using Microsoft.Win32;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace InsightLogParser.Client;

internal class UserComputer
{
    private const string IoiAppId = "2071500";

    private readonly Configuration _configuration;
    private readonly MessageWriter _messageWriter;

    private readonly Lazy<string?> _steamScreenshotFolder = new(() =>
    {
        var programFilesPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        var steamPath = Path.Combine(programFilesPath, "Steam");
        var userDataPath = Path.Combine(steamPath, "userdata");

        //760 is for Screenshots
        var subPath = Path.Combine("760", "remote", IoiAppId, "screenshots");

        if (!Directory.Exists(userDataPath)) return null;

        //Find a user profile that has screenshot folder for IoI
        var userProfiles = Directory.GetFileSystemEntries(userDataPath, "*", SearchOption.TopDirectoryOnly);
        var targetFolder = userProfiles
            .Select(profilePath => Path.Combine(profilePath, subPath))
            .FirstOrDefault(Directory.Exists);
        return targetFolder;
    });

    public UserComputer(Configuration configuration, MessageWriter messageWriter)
    {
        _configuration = configuration;
        _messageWriter = messageWriter;
    }

    public string? GetScreenshotFolder()
    {
        if (Debugger.IsAttached) return DebugOverrides.ScreenshotFolder;
        if (!string.IsNullOrWhiteSpace(_configuration.ForceScreenshotFolder))
        {
            return _configuration.ForceScreenshotFolder;
        }

        return _steamScreenshotFolder.Value;
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
            process.Dispose();
        }
        return running;
    }

    public void DeleteScreenshot(string path)
    {
        var folder = Path.GetDirectoryName(path) ?? ".";
        var screenshotFolder = GetScreenshotFolder() ?? ".";
        if (!folder.StartsWith(screenshotFolder))
        {
            throw new InvalidOperationException($"Screenshot '{path}' can only be deleted from the screenshot folder '{screenshotFolder}'");
        }

        var fileName = Path.GetFileName(path);
        var thumbFolder = Path.Combine(folder, "thumbnails");
        var thumbFile = Path.Combine(thumbFolder, fileName);

        if (File.Exists(thumbFile))
        {
            _messageWriter.WriteDebug($"Deleting file '{thumbFile}'");
            File.Delete(thumbFile);
        }

        if (File.Exists(path))
        {
            _messageWriter.WriteDebug($"Deleting file '{path}'");
            File.Delete(path);
        }
    }

    private enum VdfState
    {
        SearchingForApp,
        FoundApp,
        AdvancingToFilename,
        SkipFile,
        KeepFile,
        Done,
    }

    public int? CleanupSteamScreenshotFile()
    {
        var screenshotFolder = GetScreenshotFolder();
        if (screenshotFolder == null)
        {
            _messageWriter.WriteError("Unable to determine steam screenshot folder");
            return null;
        }

        var screenshotRootDirectory = Directory.GetParent(screenshotFolder)?.Parent?.Parent;
        if (screenshotRootDirectory == null)
        {
            _messageWriter.WriteError("Unable to determine steam screenshot 'root'");
            return null;
        }
        var screenshotRoot = screenshotRootDirectory.FullName;

        var vdfFile = Path.Combine(screenshotRoot, "screenshots.vdf");
        if (!File.Exists(vdfFile))
        {
            _messageWriter.WriteError("Unable to find the screenshots.vdf file");
            return null;
        }

        //Here we go
        var lines = File.ReadAllLines(vdfFile, Encoding.UTF8);
        var newLines = new List<string>(lines.Length);
        const string appIdLine = "\t\"" + IoiAppId + "\"";
        const string endFile = "\t\t}";
        const string endApp = "\t}";

        VdfState state = VdfState.SearchingForApp;
        var tempLines = new List<string>(10);
        var removedFiles = 0;
        foreach (var line in lines)
        {
            switch (state)
            {
                //Find the section with the IoI app id
                case VdfState.SearchingForApp:
                    newLines.Add(line);
                    if (line != appIdLine) break;
                    state = VdfState.FoundApp;
                    break;

                case VdfState.FoundApp:
                    newLines.Add(line);
                    state = VdfState.AdvancingToFilename;
                    break;

                case VdfState.AdvancingToFilename:
                    if (line == endApp)
                    {
                        newLines.Add(line);
                        state = VdfState.Done;
                        break;
                    }
                    tempLines.Add(line);
                    var match = Regex.Match(line, @"^\t\t\t\""filename""\t\t\""(.*)\""$");
                    if (!match.Success) break;
                    var partialPath = match.Groups[1].Value;
                    var pathParts = partialPath.Split('/');
                    var filePath = pathParts.Aggregate(Path.Combine(screenshotRoot, "remote"), Path.Combine);

                    if (File.Exists(filePath))
                    {
                        state = VdfState.KeepFile;
                        newLines.AddRange(tempLines);
                    }
                    else
                    {
                        removedFiles++;
                        state = VdfState.SkipFile;
                    }
                    tempLines.Clear();
                    break;

                case VdfState.KeepFile:
                    newLines.Add(line);
                    if (line != endFile) break;
                    state = VdfState.AdvancingToFilename;
                    break;

                case VdfState.SkipFile:
                    if (line != endFile) break;
                    state = VdfState.AdvancingToFilename;
                    break;

                case VdfState.Done:
                    newLines.Add(line);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        if (removedFiles > 0)
        {
            var namePart = Path.GetFileNameWithoutExtension(vdfFile);
            var extension = Path.GetExtension(vdfFile);
            var backupName = $"{namePart}-backup-{DateTime.Now:yyyyMMddHHmmss}{extension}";
            var pathPart = Path.GetDirectoryName(vdfFile) ?? "";
            var backupPath = Path.Combine(pathPart, backupName);

            _messageWriter.WriteInfo($"Removing {removedFiles} entries, backup has been saved to '{backupPath}'");

            File.Copy(vdfFile, backupPath);
            File.WriteAllText(vdfFile, string.Join('\n',newLines) + '\n', Encoding.UTF8);
        }
        else
        {
            _messageWriter.WriteInfo("Nothing to do");
        }

        return removedFiles;
    }

    public void LaunchBrowser(string httpsUrl)
    {
        if (!httpsUrl.StartsWith("https://")) throw new ArgumentException("Not a valid https URL");
        Process.Start(new ProcessStartInfo
        {
            FileName = httpsUrl,
            UseShellExecute = true,
        });
    }

}