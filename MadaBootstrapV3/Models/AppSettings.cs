namespace MadaBootstrapV3.Models;

public class AppSettings
{
    public bool AutoCloseAfterLaunch { get; set; }
    public bool WriteClientSettingsOnLaunch { get; set; }
    public string DefaultPlaceId { get; set; } = "";
    public string LastKnownClientPath { get; set; } = "";
    public string LastStatus { get; set; } = "idle";
    public string ClientSettingsJson { get; set; } =
        """
        {
          \"DFIntTaskSchedulerTargetFps\": 120,
          \"FFlagDebugGraphicsPreferD3D11\": true,
          \"FFlagHandleAltEnterFullscreenManually\": false
        }
        """;
}
