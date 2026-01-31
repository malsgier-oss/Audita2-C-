using WorkAudit.Core;

namespace WorkAudit.Services;

/// <summary>
/// Document operations with audit logging.
/// </summary>
public class DocumentService
{
    private readonly DocumentRepository _repo;
    private readonly AuditLogService _audit;
    private readonly string _actor;

    public DocumentService(string baseDir, string sessionUser, string deviceId)
    {
        _repo = new DocumentRepository(baseDir);
        _audit = new AuditLogService(baseDir);
        _actor = $"{sessionUser}@{deviceId}";
    }

    public IReadOnlyList<Document> ListDocuments(DocumentFilters? filters, int limit = 500)
        => _repo.ListDocuments(filters, limit);

    public IReadOnlyList<Document> Search(string? query, DocumentFilters? filters, int limit = 500)
        => _repo.Search(query, filters, limit);

    public Document? GetById(int id) => _repo.GetById(id);

    public void UpdateNotes(int id, string notes)
    {
        var doc = _repo.GetById(id);
        if (doc == null) return;
        _repo.UpdateNotes(id, notes);
        _audit.Append(_actor, "update", "document", doc.Uuid, id, new { notes });
    }

    public void UpdateStatus(int id, string status)
    {
        var doc = _repo.GetById(id);
        if (doc == null) return;
        var oldStatus = doc.Status;
        _repo.UpdateStatus(id, status);
        _audit.Append(_actor, "status_change", "document", doc.Uuid, id, new { oldStatus, newStatus = status });
    }

    public void MarkReviewed(int id, bool setStatusReviewed = true)
    {
        var doc = _repo.GetById(id);
        if (doc == null) return;
        _repo.MarkReviewed(id, setStatusReviewed);
        _audit.Append(_actor, "review", "document", doc.Uuid, id, new { setStatusReviewed });
    }

    public void Delete(int id)
    {
        var doc = _repo.GetById(id);
        if (doc == null) return;
        _repo.Delete(id);
        _audit.Append(_actor, "delete", "document", doc.Uuid, id, new { filePath = doc.FilePath });
    }

    public void DeleteMany(IEnumerable<int> ids)
    {
        foreach (var id in ids)
            Delete(id);
    }

    public int Insert(Document doc) => _repo.Insert(doc);
}
