using System.Diagnostics;

namespace MadaBootstrap.Services;

public class RobloxService
{
    public string GetVersionsFolder()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Roblox",
            "Versions"
        );
    }

    public string? FindLatestClient()
    {
        var versionsFolder = GetVersionsFolder();

        if (!Directory.Exists(versionsFolder))
            return null;

        var versionDirs = new DirectoryInfo(versionsFolder)
            .GetDirectories()
            .OrderByDescending(d => d.LastWriteTimeUtc)
            .ToList();

        foreach (var dir in versionDirs)
        {
            var exe = Path.Combine(dir.FullName, "RobloxPlayerBeta.exe");
            if (File.Exists(exe))
                return exe;
        }

        return null;
    }

    public bool IsInstalled()
    {
        return FindLatestClient() is not null;
    }

    public void LaunchClient(string? clientPath = null)
    {
        var resolved = string.IsNullOrWhiteSpace(clientPath) ? FindLatestClient() : clientPath;

        if (string.IsNullOrWhiteSpace(resolved) || !File.Exists(resolved))
            throw new FileNotFoundException("RobloxPlayerBeta.exe introuvable.");

        Process.Start(new ProcessStartInfo
        {
            FileName = resolved,
            UseShellExecute = true
        });
    }

    public void JoinPlace(string placeId)
    {
        if (string.IsNullOrWhiteSpace(placeId))
            throw new ArgumentException("Place ID vide.");

        Process.Start(new ProcessStartInfo
        {
            FileName = $"roblox://placeID={placeId}",
            UseShellExecute = true
        });
    }

    public string EnsureClientSettingsFolder(string? clientPath = null)
    {
        var resolved = string.IsNullOrWhiteSpace(clientPath) ? FindLatestClient() : clientPath;

        if (string.IsNullOrWhiteSpace(resolved) || !File.Exists(resolved))
            throw new FileNotFoundException("Impossible de localiser le client Roblox.");

        var versionFolder = Path.GetDirectoryName(resolved)!;
        var clientSettingsFolder = Path.Combine(versionFolder, "ClientSettings");

        if (!Directory.Exists(clientSettingsFolder))
            Directory.CreateDirectory(clientSettingsFolder);

        return clientSettingsFolder;
    }

    public string WriteClientSettings(string json, string? clientPath = null)
    {
        var folder = EnsureClientSettingsFolder(clientPath);
        var targetPath = Path.Combine(folder, "ClientAppSettings.json");

        File.WriteAllText(targetPath, json);
        return targetPath;
    }

    public void OpenRobloxFolder()
    {
        var path = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Roblox"
        );

        if (!Directory.Exists(path))
            throw new DirectoryNotFoundException("Dossier Roblox introuvable.");

        Process.Start(new ProcessStartInfo
        {
            FileName = path,
            UseShellExecute = true
        });
    }
}
