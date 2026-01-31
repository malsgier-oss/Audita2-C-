using System.Text.Json;
using WorkAudit.Core;

namespace WorkAudit.Services;

/// <summary>
/// Reads/writes user_settings.json from %APPDATA%\WORKAUDIT\
/// </summary>
public class SettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private readonly string _settingsPath;
    private UserSettings? _cached;

    public SettingsStore()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var workAuditDir = Path.Combine(appData, "WORKAUDIT");
        Directory.CreateDirectory(workAuditDir);
        _settingsPath = Path.Combine(workAuditDir, "user_settings.json");
    }

    public string SettingsPath => _settingsPath;

    public UserSettings Load()
    {
        if (_cached != null)
            return _cached;

        if (!File.Exists(_settingsPath))
        {
            _cached = CreateDefault();
            Save(_cached);
            return _cached;
        }

        try
        {
            var json = File.ReadAllText(_settingsPath);
            _cached = JsonSerializer.Deserialize<UserSettings>(json, JsonOptions) ?? CreateDefault();
        }
        catch
        {
            _cached = CreateDefault();
        }

        EnsureDeviceId();
        return _cached!;
    }

    public void Save(UserSettings settings)
    {
        EnsureDeviceId(settings);
        var dir = Path.GetDirectoryName(_settingsPath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);
        var json = JsonSerializer.Serialize(settings, JsonOptions);
        File.WriteAllText(_settingsPath, json);
        _cached = settings;
    }

    /// <summary>
    /// Ensures base_dir and base_dir\inbox exist. Uses default Documents\WORKAUDIT_Docs if base_dir empty.
    /// </summary>
    public void EnsureFoldersExist(UserSettings settings)
    {
        var baseDir = GetEffectiveBaseDir(settings);
        Directory.CreateDirectory(baseDir);
        var inboxPath = Path.Combine(baseDir, "inbox");
        Directory.CreateDirectory(inboxPath);
    }

    public string GetEffectiveBaseDir(UserSettings settings)
    {
        if (!string.IsNullOrWhiteSpace(settings.BaseDir))
            return settings.BaseDir;

        var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        return Path.Combine(documents, "WORKAUDIT_Docs");
    }

    public void InvalidateCache()
    {
        _cached = null;
    }

    private static UserSettings CreateDefault()
    {
        var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        return new UserSettings
        {
            DeviceId = Guid.NewGuid().ToString(),
            BaseDir = Path.Combine(documents, "WORKAUDIT_Docs"),
            OcrLang = "eng",
            CameraIndex = 0,
            WorkspaceSplitterSizes = [300, 450, 450, 300],
            SearchPanelVisible = true,
            Theme = "light",
            Language = "en",
            FontSize = 12
        };
    }

    private void EnsureDeviceId(UserSettings? settings = null)
    {
        var s = settings ?? _cached;
        if (s == null) return;
        if (string.IsNullOrWhiteSpace(s.DeviceId))
            s.DeviceId = Guid.NewGuid().ToString();
    }
}
