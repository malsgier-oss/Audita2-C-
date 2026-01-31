using System.Text.Json;

namespace WorkAudit.Services;

/// <summary>
/// Writes audit entries as JSONL to %APPDATA%\WORKAUDIT\audit_forward\audit.jsonl.
/// Policy: forward before DB insert; if forward fails, do not insert.
/// </summary>
public class AuditForwarder
{
    private readonly string _forwardPath;

    public AuditForwarder()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var dir = Path.Combine(appData, "WORKAUDIT", "audit_forward");
        Directory.CreateDirectory(dir);
        _forwardPath = Path.Combine(dir, "audit.jsonl");
    }

    /// <summary>
    /// Forwards the audit entry. Returns true if successful.
    /// </summary>
    public bool Forward(AuditEntry entry)
    {
        try
        {
            var json = JsonSerializer.Serialize(entry);
            File.AppendAllText(_forwardPath, json + Environment.NewLine);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
