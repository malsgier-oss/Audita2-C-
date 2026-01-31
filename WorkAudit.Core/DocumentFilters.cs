namespace WorkAudit.Core;

/// <summary>
/// Filters for document listing and search.
/// </summary>
public class DocumentFilters
{
    public string? Section { get; set; }
    public string? Branch { get; set; }
    public string? DocumentType { get; set; }
    public string? Status { get; set; }
    public string? DateFrom { get; set; }
    public string? DateTo { get; set; }
}
