using System.Text.Json;
using FinspectorPoC.Models;

namespace FinspectorPoC.Services;

/// <summary>
/// Loads and saves AppSettings to a local JSON file that is gitignored.
/// </summary>
public class LocalSettingsService
{
    private static readonly JsonSerializerOptions _json = new() { WriteIndented = true };

    public string FilePath { get; }

    public LocalSettingsService(IConfiguration configuration)
    {
        // Default to appsettings.local.json next to the executable
        var dir = AppContext.BaseDirectory;
        FilePath = Path.Combine(dir, "appsettings.local.json");
    }

    public AppSettings Load()
    {
        if (!File.Exists(FilePath))
            return new AppSettings();

        try
        {
            var json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    public void Save(AppSettings settings)
    {
        var json = JsonSerializer.Serialize(settings, _json);
        File.WriteAllText(FilePath, json);
    }
}
