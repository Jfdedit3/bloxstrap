namespace MadaBootstrap.Models;

public class AppSettings
{
    public bool AutoCloseAfterLaunch { get; set; }
    public bool UseProtocolForPlaceJoin { get; set; } = true;
    public bool WriteClientSettingsOnLaunch { get; set; }
    public string DefaultPlaceId { get; set; } = "";
    public string LastKnownClientPath { get; set; } = "";
    public string ClientSettingsJson { get; set; } =
        """
        {
          \"DFIntTaskSchedulerTargetFps\": 120,
          \"FFlagDebugGraphicsPreferD3D11\": true
        }
        """;
}
