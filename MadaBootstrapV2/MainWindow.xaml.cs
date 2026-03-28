using System.Windows;
using MadaBootstrapV2.Models;
using MadaBootstrapV2.Services;

namespace MadaBootstrapV2;

public partial class MainWindow : Window
{
    private readonly SettingsService _settingsService = new();
    private readonly RobloxService _robloxService = new();
    private AppSettings _settings = new();

    private const string DefaultJson =
        """
        {
          "DFIntTaskSchedulerTargetFps": 120,
          "FFlagDebugGraphicsPreferD3D11": true,
          "FFlagHandleAltEnterFullscreenManually": false
        }
        """;

    public MainWindow()
    {
        InitializeComponent();
        LoadSettings();
        DetectClient();
    }

    private void LoadSettings()
    {
        _settings = _settingsService.Load();
        AutoCloseCheck.IsChecked = _settings.AutoCloseAfterLaunch;
        WriteClientSettingsCheck.IsChecked = _settings.WriteClientSettingsOnLaunch;
        PlaceIdBox.Text = _settings.DefaultPlaceId;
        ClientPathBox.Text = _settings.LastKnownClientPath;
        ClientSettingsBox.Text = _settings.ClientSettingsJson;
        SetStatus(_settings.LastStatus);
        UpdateUiSummaries();
        SetFooter($"Config file: {_settingsService.SettingsPath}");
    }

    private void SaveSettings()
    {
        _settings.AutoCloseAfterLaunch = AutoCloseCheck.IsChecked == true;
        _settings.WriteClientSettingsOnLaunch = WriteClientSettingsCheck.IsChecked == true;
        _settings.DefaultPlaceId = PlaceIdBox.Text.Trim();
        _settings.LastKnownClientPath = ClientPathBox.Text.Trim();
        _settings.ClientSettingsJson = ClientSettingsBox.Text;
        _settings.LastStatus = StatusText.Text.Replace("Status: ", string.Empty);
        _settingsService.Save(_settings);
        UpdateUiSummaries();
    }

    private void SetStatus(string value)
    {
        StatusText.Text = $"Status: {value}";
        TimeText.Text = $"Last action: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
    }

    private void SetFooter(string value)
    {
        FooterText.Text = value;
    }

    private void UpdateUiSummaries()
    {
        ClientSummaryText.Text = string.IsNullOrWhiteSpace(ClientPathBox.Text) ? "Client path empty" : Path.GetFileName(ClientPathBox.Text);
        JsonSummaryText.Text = $"{ClientSettingsBox.Text.Length} characters in editor";
        PlaceSummaryText.Text = string.IsNullOrWhiteSpace(PlaceIdBox.Text) ? "No place selected" : $"Place ID: {PlaceIdBox.Text}";
        CurrentPathText.Text = string.IsNullOrWhiteSpace(ClientPathBox.Text) ? "No path loaded" : ClientPathBox.Text;

        try
        {
            BackupPathText.Text = _robloxService.GetBackupPath(ClientPathBox.Text.Trim());
        }
        catch
        {
            BackupPathText.Text = "Backup path unavailable until a client is detected.";
        }
    }

    private void RunSafe(Action action)
    {
        try
        {
            action();
            SaveSettings();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "MadaBootstrap V2", MessageBoxButton.OK, MessageBoxImage.Error);
            SetStatus("error");
            SetFooter(ex.Message);
        }
    }

    private void DetectClient()
    {
        var found = _robloxService.FindLatestClient();

        if (string.IsNullOrWhiteSpace(found))
        {
            SetStatus("client not found");
            SetFooter("Roblox client was not detected in LocalAppData.");
            ClientSummaryText.Text = "Client not found";
            return;
        }

        ClientPathBox.Text = found;
        SetStatus("client detected");
        SetFooter(found);
        UpdateUiSummaries();
    }

    private void LaunchWithOptionalSettings()
    {
        var clientPath = ClientPathBox.Text.Trim();

        if (WriteClientSettingsCheck.IsChecked == true)
        {
            var writtenPath = _robloxService.WriteClientSettings(ClientSettingsBox.Text, clientPath);
            SetFooter($"Wrote client settings to: {writtenPath}");
        }

        _robloxService.LaunchClient(clientPath);
        SetStatus("launched");

        if (AutoCloseCheck.IsChecked == true)
            Close();
    }

    private void DetectClientButton_Click(object sender, RoutedEventArgs e)
    {
        RunSafe(() =>
        {
            DetectClient();
        });
    }

    private void LaunchRobloxButton_Click(object sender, RoutedEventArgs e)
    {
        RunSafe(() =>
        {
            LaunchWithOptionalSettings();
        });
    }

    private void JoinPlaceButton_Click(object sender, RoutedEventArgs e)
    {
        RunSafe(() =>
        {
            _robloxService.JoinPlace(PlaceIdBox.Text.Trim());
            SetStatus("place join requested");
            SetFooter($"Requested join for place {PlaceIdBox.Text.Trim()}");
            PlaceSummaryText.Text = $"Last join: {PlaceIdBox.Text.Trim()}";

            if (AutoCloseCheck.IsChecked == true)
                Close();
        });
    }

    private void OpenRobloxFolderButton_Click(object sender, RoutedEventArgs e)
    {
        RunSafe(() =>
        {
            _robloxService.OpenRobloxFolder();
            SetStatus("roblox folder opened");
            SetFooter(_robloxService.GetRobloxFolder());
        });
    }

    private void OpenClientFolderButton_Click(object sender, RoutedEventArgs e)
    {
        RunSafe(() =>
        {
            _robloxService.OpenCurrentClientFolder(ClientPathBox.Text.Trim());
            SetStatus("client folder opened");
            SetFooter(_robloxService.GetCurrentClientFolder(ClientPathBox.Text.Trim()));
        });
    }

    private void OpenClientSettingsFolderButton_Click(object sender, RoutedEventArgs e)
    {
        RunSafe(() =>
        {
            _robloxService.OpenClientSettingsFolder(ClientPathBox.Text.Trim());
            SetStatus("clientsettings folder opened");
            SetFooter(_robloxService.EnsureClientSettingsFolder(ClientPathBox.Text.Trim()));
        });
    }

    private void OpenLogsFolderButton_Click(object sender, RoutedEventArgs e)
    {
        RunSafe(() =>
        {
            _robloxService.OpenLogsFolder();
            SetStatus("logs folder opened");
            SetFooter(_robloxService.GetLogsFolder());
        });
    }

    private void BackupButton_Click(object sender, RoutedEventArgs e)
    {
        RunSafe(() =>
        {
            var backup = _robloxService.BackupClientSettings(ClientPathBox.Text.Trim());
            BackupSummaryText.Text = $"Backup created: {Path.GetFileName(backup)}";
            SetStatus("backup created");
            SetFooter(backup);
        });
    }

    private void RestoreButton_Click(object sender, RoutedEventArgs e)
    {
        RunSafe(() =>
        {
            var restored = _robloxService.RestoreClientSettings(ClientPathBox.Text.Trim());
            ClientSettingsBox.Text = _robloxService.ReadCurrentClientSettings(ClientPathBox.Text.Trim());
            BackupSummaryText.Text = "Backup restored to current client settings";
            SetStatus("backup restored");
            SetFooter(restored);
            UpdateUiSummaries();
        });
    }

    private void FormatJsonButton_Click(object sender, RoutedEventArgs e)
    {
        RunSafe(() =>
        {
            using var doc = JsonDocument.Parse(ClientSettingsBox.Text);
            ClientSettingsBox.Text = JsonSerializer.Serialize(doc.RootElement, new JsonSerializerOptions { WriteIndented = true });
            JsonSummaryText.Text = "JSON formatted successfully";
            SetStatus("json formatted");
            SetFooter("The editor JSON was formatted.");
        });
    }

    private void ResetJsonButton_Click(object sender, RoutedEventArgs e)
    {
        RunSafe(() =>
        {
            ClientSettingsBox.Text = DefaultJson;
            JsonSummaryText.Text = "Editor reset to default JSON";
            SetStatus("json reset");
            SetFooter("Default JSON loaded into the editor.");
        });
    }

    private void CopyClientPathButton_Click(object sender, RoutedEventArgs e)
    {
        RunSafe(() =>
        {
            Clipboard.SetText(ClientPathBox.Text.Trim());
            SetStatus("client path copied");
            SetFooter("Client path copied to clipboard.");
        });
    }

    private void LoadCurrentJsonButton_Click(object sender, RoutedEventArgs e)
    {
        RunSafe(() =>
        {
            ClientSettingsBox.Text = _robloxService.ReadCurrentClientSettings(ClientPathBox.Text.Trim());
            JsonSummaryText.Text = "Live client JSON loaded into editor";
            SetStatus("live json loaded");
            SetFooter("Loaded current ClientAppSettings.json from Roblox.");
            UpdateUiSummaries();
        });
    }

    private void SaveSettingsButton_Click(object sender, RoutedEventArgs e)
    {
        RunSafe(() =>
        {
            SetStatus("settings saved");
            SetFooter("Settings saved successfully.");
        });
    }

    private void WriteSettingsNowButton_Click(object sender, RoutedEventArgs e)
    {
        RunSafe(() =>
        {
            var writtenPath = _robloxService.WriteClientSettings(ClientSettingsBox.Text, ClientPathBox.Text.Trim());
            SetStatus("client settings written");
            SetFooter(writtenPath);
            JsonSummaryText.Text = "Editor JSON written to Roblox";
        });
    }
}
