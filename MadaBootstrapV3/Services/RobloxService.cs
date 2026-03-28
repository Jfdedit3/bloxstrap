using System.Diagnostics;

namespace MadaBootstrapV3.Services;

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

    public string GetRobloxFolder()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Roblox"
        );
    }

    public string GetLogsFolder()
    {
        return Path.Combine(GetRobloxFolder(), "logs");
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

    public string GetCurrentClientFolder(string? clientPath = null)
    {
        var resolved = string.IsNullOrWhiteSpace(clientPath) ? FindLatestClient() : clientPath;

        if (string.IsNullOrWhiteSpace(resolved) || !File.Exists(resolved))
            throw new FileNotFoundException("Impossible de localiser le client Roblox.");

        return Path.GetDirectoryName(resolved)!;
    }

    public string EnsureClientSettingsFolder(string? clientPath = null)
    {
        var versionFolder = GetCurrentClientFolder(clientPath);
        var clientSettingsFolder = Path.Combine(versionFolder, "ClientSettings");

        if (!Directory.Exists(clientSettingsFolder))
            Directory.CreateDirectory(clientSettingsFolder);

        return clientSettingsFolder;
    }

    public string GetClientSettingsPath(string? clientPath = null)
    {
        return Path.Combine(EnsureClientSettingsFolder(clientPath), "ClientAppSettings.json");
    }

    public string WriteClientSettings(string json, string? clientPath = null)
    {
        var targetPath = GetClientSettingsPath(clientPath);
        File.WriteAllText(targetPath, json);
        return targetPath;
    }

    public string ReadCurrentClientSettings(string? clientPath = null)
    {
        var path = GetClientSettingsPath(clientPath);

        if (!File.Exists(path))
            return "{}";

        return File.ReadAllText(path);
    }

    public string GetBackupPath(string? clientPath = null)
    {
        return Path.Combine(EnsureClientSettingsFolder(clientPath), "ClientAppSettings.backup.json");
    }

    public string BackupClientSettings(string? clientPath = null)
    {
        var source = GetClientSettingsPath(clientPath);
        var backup = GetBackupPath(clientPath);

        if (!File.Exists(source))
            throw new FileNotFoundException("Aucun ClientAppSettings.json à sauvegarder.");

        File.Copy(source, backup, true);
        return backup;
    }

    public string RestoreClientSettings(string? clientPath = null)
    {
        var target = GetClientSettingsPath(clientPath);
        var backup = GetBackupPath(clientPath);

        if (!File.Exists(backup))
            throw new FileNotFoundException("Aucune sauvegarde trouvée.");

        File.Copy(backup, target, true);
        return target;
    }

    public void OpenRobloxFolder()
    {
        var path = GetRobloxFolder();

        if (!Directory.Exists(path))
            throw new DirectoryNotFoundException("Dossier Roblox introuvable.");

        Process.Start(new ProcessStartInfo
        {
            FileName = path,
            UseShellExecute = true
        });
    }

    public void OpenLogsFolder()
    {
        var path = GetLogsFolder();

        if (!Directory.Exists(path))
            throw new DirectoryNotFoundException("Dossier logs introuvable.");

        Process.Start(new ProcessStartInfo
        {
            FileName = path,
            UseShellExecute = true
        });
    }

    public void OpenCurrentClientFolder(string? clientPath = null)
    {
        var path = GetCurrentClientFolder(clientPath);

        Process.Start(new ProcessStartInfo
        {
            FileName = path,
            UseShellExecute = true
        });
    }

    public void OpenClientSettingsFolder(string? clientPath = null)
    {
        var path = EnsureClientSettingsFolder(clientPath);

        Process.Start(new ProcessStartInfo
        {
            FileName = path,
            UseShellExecute = true
        });
    }
}
