using Microsoft.Data.Sqlite;
using WorkAudit.Core;

namespace WorkAudit.Data;

/// <summary>
/// Document CRUD and search operations against workaudit.db.
/// </summary>
public class DocumentRepository
{
    private readonly string _dbPath;

    public DocumentRepository(string baseDir)
    {
        _dbPath = Path.Combine(baseDir, "workaudit.db");
    }

    public IReadOnlyList<Document> ListDocuments(DocumentFilters? filters, int limit = 500)
    {
        var (sql, parameters) = BuildListQuery(filters, limit);
        return ExecuteQuery(sql, parameters, ReadDocument);
    }

    public IReadOnlyList<Document> Search(string? query, DocumentFilters? filters, int limit = 500)
    {
        var (sql, parameters) = BuildSearchQuery(query, filters, limit);
        return ExecuteQuery(sql, parameters, ReadDocument);
    }

    public Document? GetById(int id)
    {
        var list = ExecuteQuery(
            "SELECT * FROM documents WHERE id = @id",
            [new SqliteParameter("@id", id)],
            ReadDocument);
        return list.FirstOrDefault();
    }

    public void UpdateNotes(int id, string notes)
    {
        var now = DateTime.UtcNow.ToString("o");
        ExecuteNonQuery(
            "UPDATE documents SET notes = @notes, updated_at = @updated WHERE id = @id",
            [
                new SqliteParameter("@notes", notes ?? ""),
                new SqliteParameter("@updated", now),
                new SqliteParameter("@id", id)
            ]);
    }

    public void UpdateStatus(int id, string status)
    {
        var now = DateTime.UtcNow.ToString("o");
        ExecuteNonQuery(
            "UPDATE documents SET status = @status, updated_at = @updated WHERE id = @id",
            [
                new SqliteParameter("@status", status),
                new SqliteParameter("@updated", now),
                new SqliteParameter("@id", id)
            ]);
    }

    public void MarkReviewed(int id, bool setStatusReviewed = true)
    {
        var now = DateTime.UtcNow.ToString("o");
        var status = setStatusReviewed ? "Reviewed" : null;
        if (status != null)
        {
            ExecuteNonQuery(
                "UPDATE documents SET status = @status, reviewed_at = @reviewed, updated_at = @updated WHERE id = @id",
                [
                    new SqliteParameter("@status", status),
                    new SqliteParameter("@reviewed", now),
                    new SqliteParameter("@updated", now),
                    new SqliteParameter("@id", id)
                ]);
        }
        else
        {
            ExecuteNonQuery(
                "UPDATE documents SET reviewed_at = @reviewed, updated_at = @updated WHERE id = @id",
                [
                    new SqliteParameter("@reviewed", now),
                    new SqliteParameter("@updated", now),
                    new SqliteParameter("@id", id)
                ]);
        }
    }

    public void Delete(int id)
    {
        ExecuteNonQuery("DELETE FROM documents WHERE id = @id", [new SqliteParameter("@id", id)]);
    }

    public void DeleteMany(IEnumerable<int> ids)
    {
        var idList = ids.ToList();
        if (idList.Count == 0) return;
        var placeholders = string.Join(",", idList.Select((_, i) => $"@p{i}"));
        var cmd = $"DELETE FROM documents WHERE id IN ({placeholders})";
        var parameters = idList.Select((id, i) => new SqliteParameter($"@p{i}", id)).ToArray();
        ExecuteNonQuery(cmd, parameters);
    }

    public int Insert(Document doc)
    {
        var now = DateTime.UtcNow.ToString("o");
        var uuid = string.IsNullOrEmpty(doc.Uuid) ? Guid.NewGuid().ToString() : doc.Uuid;
        ExecuteNonQuery(@"
            INSERT INTO documents (uuid, file_path, document_type, extracted_date, amounts, snippet, ocr_text,
                capture_time, source, engagement, section, clearing_direction, clearing_status, notes,
                confidence, status, branch, created_by_user, created_by_device, created_at, sync_status, sync_version)
            VALUES (@uuid, @file_path, @doc_type, @extracted_date, @amounts, @snippet, @ocr_text,
                @capture_time, @source, @engagement, @section, @clearing_dir, @clearing_status, @notes,
                @confidence, @status, @branch, @created_user, @created_device, @created_at, 'local_only', 1)",
            [
                new SqliteParameter("@uuid", uuid),
                new SqliteParameter("@file_path", doc.FilePath),
                new SqliteParameter("@doc_type", doc.DocumentType ?? (object)DBNull.Value),
                new SqliteParameter("@extracted_date", doc.ExtractedDate ?? (object)DBNull.Value),
                new SqliteParameter("@amounts", doc.Amounts ?? (object)DBNull.Value),
                new SqliteParameter("@snippet", doc.Snippet ?? (object)DBNull.Value),
                new SqliteParameter("@ocr_text", doc.OcrText ?? (object)DBNull.Value),
                new SqliteParameter("@capture_time", doc.CaptureTime),
                new SqliteParameter("@source", doc.Source),
                new SqliteParameter("@engagement", doc.Engagement),
                new SqliteParameter("@section", doc.Section),
                new SqliteParameter("@clearing_dir", doc.ClearingDirection ?? (object)DBNull.Value),
                new SqliteParameter("@clearing_status", doc.ClearingStatus ?? (object)DBNull.Value),
                new SqliteParameter("@notes", doc.Notes ?? (object)DBNull.Value),
                new SqliteParameter("@confidence", doc.Confidence),
                new SqliteParameter("@status", doc.Status),
                new SqliteParameter("@branch", doc.Branch ?? (object)DBNull.Value),
                new SqliteParameter("@created_user", doc.CreatedByUser ?? (object)DBNull.Value),
                new SqliteParameter("@created_device", doc.CreatedByDevice ?? (object)DBNull.Value),
                new SqliteParameter("@created_at", now)
            ]);
        return GetLastInsertId();
    }

    private int GetLastInsertId()
    {
        using var conn = new SqliteConnection($"Data Source={_dbPath}");
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT last_insert_rowid()";
        return Convert.ToInt32(cmd.ExecuteScalar()!);
    }

    private (string sql, SqliteParameter[] parameters) BuildListQuery(DocumentFilters? filters, int limit)
    {
        var where = new List<string> { "1=1" };
        var parameters = new List<SqliteParameter>();
        if (filters != null)
        {
            if (!string.IsNullOrEmpty(filters.Section)) { where.Add("section = @section"); parameters.Add(new SqliteParameter("@section", filters.Section)); }
            if (!string.IsNullOrEmpty(filters.Branch)) { where.Add("branch = @branch"); parameters.Add(new SqliteParameter("@branch", filters.Branch)); }
            if (!string.IsNullOrEmpty(filters.DocumentType)) { where.Add("document_type = @doc_type"); parameters.Add(new SqliteParameter("@doc_type", filters.DocumentType)); }
            if (!string.IsNullOrEmpty(filters.Status)) { where.Add("status = @status"); parameters.Add(new SqliteParameter("@status", filters.Status)); }
            if (!string.IsNullOrEmpty(filters.DateFrom)) { where.Add("capture_time >= @date_from"); parameters.Add(new SqliteParameter("@date_from", filters.DateFrom)); }
            if (!string.IsNullOrEmpty(filters.DateTo)) { where.Add("capture_time <= @date_to"); parameters.Add(new SqliteParameter("@date_to", filters.DateTo)); }
        }
        parameters.Add(new SqliteParameter("@limit", limit));
        var sql = $"SELECT * FROM documents WHERE {string.Join(" AND ", where)} ORDER BY capture_time DESC LIMIT @limit";
        return (sql, [.. parameters]);
    }

    private (string sql, SqliteParameter[] parameters) BuildSearchQuery(string? query, DocumentFilters? filters, int limit)
    {
        var where = new List<string> { "1=1" };
        var parameters = new List<SqliteParameter>();
        if (!string.IsNullOrWhiteSpace(query))
        {
            var pattern = $"%{query.Trim()}%";
            where.Add("(ocr_text LIKE @q OR snippet LIKE @q OR notes LIKE @q OR file_path LIKE @q)");
            parameters.Add(new SqliteParameter("@q", pattern));
        }
        if (filters != null)
        {
            if (!string.IsNullOrEmpty(filters.Section)) { where.Add("section = @section"); parameters.Add(new SqliteParameter("@section", filters.Section)); }
            if (!string.IsNullOrEmpty(filters.Branch)) { where.Add("branch = @branch"); parameters.Add(new SqliteParameter("@branch", filters.Branch)); }
            if (!string.IsNullOrEmpty(filters.DocumentType)) { where.Add("document_type = @doc_type"); parameters.Add(new SqliteParameter("@doc_type", filters.DocumentType)); }
            if (!string.IsNullOrEmpty(filters.Status)) { where.Add("status = @status"); parameters.Add(new SqliteParameter("@status", filters.Status)); }
            if (!string.IsNullOrEmpty(filters.DateFrom)) { where.Add("capture_time >= @date_from"); parameters.Add(new SqliteParameter("@date_from", filters.DateFrom)); }
            if (!string.IsNullOrEmpty(filters.DateTo)) { where.Add("capture_time <= @date_to"); parameters.Add(new SqliteParameter("@date_to", filters.DateTo)); }
        }
        parameters.Add(new SqliteParameter("@limit", limit));
        var sql = $"SELECT * FROM documents WHERE {string.Join(" AND ", where)} ORDER BY capture_time DESC LIMIT @limit";
        return (sql, [.. parameters]);
    }

    private List<Document> ExecuteQuery(string sql, SqliteParameter[] parameters, Func<SqliteDataReader, Document> reader)
    {
        var list = new List<Document>();
        using var conn = new SqliteConnection($"Data Source={_dbPath}");
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.Parameters.AddRange(parameters);
        using var r = cmd.ExecuteReader();
        while (r.Read()) list.Add(reader(r));
        return list;
    }

    private void ExecuteNonQuery(string sql, SqliteParameter[]? parameters = null)
    {
        using var conn = new SqliteConnection($"Data Source={_dbPath}");
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        if (parameters != null) cmd.Parameters.AddRange(parameters);
        cmd.ExecuteNonQuery();
    }

    private static Document ReadDocument(SqliteDataReader r)
    {
        return new Document
        {
            Id = r.GetInt32(r.GetOrdinal("id")),
            Uuid = r.IsDBNull(r.GetOrdinal("uuid")) ? "" : r.GetString(r.GetOrdinal("uuid")),
            FilePath = r.GetString(r.GetOrdinal("file_path")),
            DocumentType = r.IsDBNull(r.GetOrdinal("document_type")) ? null : r.GetString(r.GetOrdinal("document_type")),
            ExtractedDate = r.IsDBNull(r.GetOrdinal("extracted_date")) ? null : r.GetString(r.GetOrdinal("extracted_date")),
            Amounts = r.IsDBNull(r.GetOrdinal("amounts")) ? null : r.GetString(r.GetOrdinal("amounts")),
            Snippet = r.IsDBNull(r.GetOrdinal("snippet")) ? null : r.GetString(r.GetOrdinal("snippet")),
            OcrText = r.IsDBNull(r.GetOrdinal("ocr_text")) ? null : r.GetString(r.GetOrdinal("ocr_text")),
            CaptureTime = r.GetString(r.GetOrdinal("capture_time")),
            Source = r.GetString(r.GetOrdinal("source")),
            Engagement = r.GetString(r.GetOrdinal("engagement")),
            Section = r.GetString(r.GetOrdinal("section")),
            ClearingDirection = r.IsDBNull(r.GetOrdinal("clearing_direction")) ? null : r.GetString(r.GetOrdinal("clearing_direction")),
            ClearingStatus = r.IsDBNull(r.GetOrdinal("clearing_status")) ? null : r.GetString(r.GetOrdinal("clearing_status")),
            Notes = r.IsDBNull(r.GetOrdinal("notes")) ? null : r.GetString(r.GetOrdinal("notes")),
            Confidence = r.GetDouble(r.GetOrdinal("confidence")),
            Status = r.GetString(r.GetOrdinal("status")),
            ReviewedAt = r.IsDBNull(r.GetOrdinal("reviewed_at")) ? null : r.GetString(r.GetOrdinal("reviewed_at")),
            UpdatedAt = r.IsDBNull(r.GetOrdinal("updated_at")) ? null : r.GetString(r.GetOrdinal("updated_at")),
            Branch = r.IsDBNull(r.GetOrdinal("branch")) ? null : r.GetString(r.GetOrdinal("branch")),
            CreatedByUser = r.IsDBNull(r.GetOrdinal("created_by_user")) ? null : r.GetString(r.GetOrdinal("created_by_user")),
            CreatedByDevice = r.IsDBNull(r.GetOrdinal("created_by_device")) ? null : r.GetString(r.GetOrdinal("created_by_device")),
            CreatedAt = r.IsDBNull(r.GetOrdinal("created_at")) ? null : r.GetString(r.GetOrdinal("created_at")),
            SyncStatus = r.GetString(r.GetOrdinal("sync_status")),
            SyncVersion = r.GetInt32(r.GetOrdinal("sync_version")),
            StorageKey = r.IsDBNull(r.GetOrdinal("storage_key")) ? null : r.GetString(r.GetOrdinal("storage_key"))
        };
    }
}
