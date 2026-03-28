using System.Windows;
using MadaBootstrap.Models;
using MadaBootstrap.Services;

namespace MadaBootstrap;

public partial class MainWindow : Window
{
    private readonly SettingsService _settingsService = new();
    private readonly RobloxService _robloxService = new();
    private AppSettings _settings = new();

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
        UseProtocolCheck.IsChecked = _settings.UseProtocolForPlaceJoin;
        WriteClientSettingsCheck.IsChecked = _settings.WriteClientSettingsOnLaunch;
        PlaceIdBox.Text = _settings.DefaultPlaceId;
        ClientPathBox.Text = _settings.LastKnownClientPath;
        ClientSettingsBox.Text = _settings.ClientSettingsJson;

        SetStatus("settings loaded");
        SetFooter($"Config file: {_settingsService.SettingsPath}");
    }

    private void SaveSettings()
    {
        _settings.AutoCloseAfterLaunch = AutoCloseCheck.IsChecked == true;
        _settings.UseProtocolForPlaceJoin = UseProtocolCheck.IsChecked == true;
        _settings.WriteClientSettingsOnLaunch = WriteClientSettingsCheck.IsChecked == true;
        _settings.DefaultPlaceId = PlaceIdBox.Text.Trim();
        _settings.LastKnownClientPath = ClientPathBox.Text.Trim();
        _settings.ClientSettingsJson = ClientSettingsBox.Text;

        _settingsService.Save(_settings);
        SetStatus("settings saved");
        SetFooter("Settings saved successfully.");
    }

    private void DetectClient()
    {
        var found = _robloxService.FindLatestClient();

        if (!string.IsNullOrWhiteSpace(found))
        {
            ClientPathBox.Text = found;
            _settings.LastKnownClientPath = found;
            _settingsService.Save(_settings);
            SetStatus("client detected");
            SetFooter(found);
        }
        else
        {
            SetStatus("client not found");
            SetFooter("Roblox client was not detected in LocalAppData.");
        }
    }

    private void SetStatus(string value)
    {
        StatusText.Text = $"Status: {value}";
    }

    private void SetFooter(string value)
    {
        FooterText.Text = value;
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

    private void DetectButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            DetectClient();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "MadaBootstrap", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void LaunchButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            SaveSettings();
            LaunchWithOptionalSettings();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "MadaBootstrap", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OpenFolderButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _robloxService.OpenRobloxFolder();
            SetStatus("folder opened");
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "MadaBootstrap", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void JoinPlaceButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            SaveSettings();
            var placeId = PlaceIdBox.Text.Trim();
            _robloxService.JoinPlace(placeId);
            SetStatus("place join requested");

            if (AutoCloseCheck.IsChecked == true)
                Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "MadaBootstrap", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            SaveSettings();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "MadaBootstrap", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void WriteSettingsNowButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            SaveSettings();
            var writtenPath = _robloxService.WriteClientSettings(ClientSettingsBox.Text, ClientPathBox.Text.Trim());
            SetStatus("client settings written");
            SetFooter(writtenPath);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "MadaBootstrap", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
