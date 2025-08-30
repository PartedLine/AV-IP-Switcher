using System.IO;
using System.Text.Json;

namespace IpSwitcher2.Classes;

public class AppConfig
{
    public bool FirstRun { get; set; } = true;
}

public static class ConfigManager
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };
    
    private static string ConfigPath => Path.Combine(FileService.AppDataPath, "config.json");
    private static AppConfig? _config = new();
    
    public static AppConfig Config => _config!;

    public static void LoadConfig()
    {
        if (File.Exists(ConfigPath))
        {
            var jsonString = File.ReadAllText(ConfigPath);
            _config = JsonSerializer.Deserialize <AppConfig>(jsonString) ?? new AppConfig();
        }
        else
        {
            SaveConfig(); // Create default config file
        }
    }

    public static void SaveConfig()
    {
        var jsonString = JsonSerializer.Serialize(_config, Options);
        File.WriteAllText(ConfigPath, jsonString);
    }
}