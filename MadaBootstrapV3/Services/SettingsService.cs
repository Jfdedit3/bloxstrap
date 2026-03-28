using System.Text.Json;
using MadaBootstrapV3.Models;

namespace MadaBootstrapV3.Services;

public class SettingsService
{
    private readonly string _appFolder;
    private readonly string _settingsPath;

    public SettingsService()
    {
        _appFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "MadaBootstrapV3"
        );

        _settingsPath = Path.Combine(_appFolder, "settings.json");
    }

    public string AppFolder => _appFolder;
    public string SettingsPath => _settingsPath;

    public AppSettings Load()
    {
        try
        {
            if (!Directory.Exists(_appFolder))
                Directory.CreateDirectory(_appFolder);

            if (!File.Exists(_settingsPath))
            {
                var defaults = new AppSettings();
                Save(defaults);
                return defaults;
            }

            var json = File.ReadAllText(_settingsPath);
            var settings = JsonSerializer.Deserialize<AppSettings>(json);
            return settings ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    public void Save(AppSettings settings)
    {
        if (!Directory.Exists(_appFolder))
            Directory.CreateDirectory(_appFolder);

        var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(_settingsPath, json);
    }
}
