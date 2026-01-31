namespace WorkAudit.Services;

/// <summary>
/// Audit log entry for JSONL forward and DB.
/// </summary>
public class AuditEntry
{
    public string Uuid { get; set; } = string.Empty;
    public string Timestamp { get; set; } = string.Empty;
    public string Actor { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string? EntityUuid { get; set; }
    public int? EntityId { get; set; }
    public string? Details { get; set; }
    public string PrevHash { get; set; } = string.Empty;
}
