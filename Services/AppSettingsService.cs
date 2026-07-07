using System.IO;
using System.Text;
using System.Text.Json;

namespace SmartOpsDesk.Services;

public static class AppSettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static string SettingsFile
    {
        get
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var directory = Path.Combine(appData, "SmartOpsDesk");
            Directory.CreateDirectory(directory);
            return Path.Combine(directory, "settings.json");
        }
    }

    public static bool Exists => File.Exists(SettingsFile);

    public static AppSettings Load()
    {
        if (!File.Exists(SettingsFile))
        {
            return new AppSettings();
        }

        try
        {
            var json = File.ReadAllText(SettingsFile, Encoding.UTF8);
            return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    public static void Save(AppSettings settings)
    {
        var json = JsonSerializer.Serialize(settings, JsonOptions);
        File.WriteAllText(SettingsFile, json, Encoding.UTF8);
    }
}
