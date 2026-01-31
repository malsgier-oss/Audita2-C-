namespace WorkAudit.Core;

/// <summary>
/// Strongly typed user settings persisted to %APPDATA%\WORKAUDIT\user_settings.json
/// </summary>
public class UserSettings
{
    public string DeviceId { get; set; } = string.Empty;
    public string BaseDir { get; set; } = string.Empty;
    public string OcrLang { get; set; } = "eng";
    public int CameraIndex { get; set; }
    public int[] WorkspaceSplitterSizes { get; set; } = [300, 450, 450, 300];
    public bool SearchPanelVisible { get; set; } = true;
    public string Theme { get; set; } = "light";
    public string Language { get; set; } = "en";
    public int FontSize { get; set; } = 12;
}
