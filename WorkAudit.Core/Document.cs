namespace WorkAudit.Core;

/// <summary>
/// Document entity matching AS-IS documents table schema.
/// </summary>
public class Document
{
    public int Id { get; set; }
    public string Uuid { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string? DocumentType { get; set; }
    public string? ExtractedDate { get; set; }
    public string? Amounts { get; set; }
    public string? Snippet { get; set; }
    public string? OcrText { get; set; }
    public string CaptureTime { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Engagement { get; set; } = string.Empty;
    public string Section { get; set; } = string.Empty; // Individuals, Companies, Clearing
    public string? ClearingDirection { get; set; }
    public string? ClearingStatus { get; set; }
    public string? Notes { get; set; }
    public double Confidence { get; set; }
    public string Status { get; set; } = "Draft";
    public string? ReviewedAt { get; set; }
    public string? UpdatedAt { get; set; }
    public string? Branch { get; set; }
    public string? CreatedByUser { get; set; }
    public string? CreatedByDevice { get; set; }
    public string? CreatedAt { get; set; }
    public string SyncStatus { get; set; } = "local_only";
    public int SyncVersion { get; set; } = 1;
    public string? StorageKey { get; set; }
}
