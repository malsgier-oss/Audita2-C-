using Microsoft.Data.Sqlite;

namespace WorkAudit.Data;

/// <summary>
/// Raw SQL migrations for workaudit.db and workaudit_audit.db.
/// </summary>
public static class DatabaseMigrator
{
    public static void EnsureMainDb(string baseDir)
    {
        var dbPath = Path.Combine(baseDir, "workaudit.db");
        using var conn = new SqliteConnection($"Data Source={dbPath}");
        conn.Open();

        ExecuteSql(conn, @"
            CREATE TABLE IF NOT EXISTS documents (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                uuid TEXT UNIQUE,
                file_path TEXT NOT NULL,
                document_type TEXT,
                extracted_date TEXT,
                amounts TEXT,
                snippet TEXT,
                ocr_text TEXT,
                capture_time TEXT NOT NULL,
                source TEXT NOT NULL,
                engagement TEXT NOT NULL,
                section TEXT NOT NULL CHECK(section IN ('Individuals','Companies','Clearing')),
                clearing_direction TEXT,
                clearing_status TEXT,
                notes TEXT,
                confidence REAL DEFAULT 0.0,
                status TEXT NOT NULL DEFAULT 'Draft',
                reviewed_at TEXT,
                updated_at TEXT,
                branch TEXT,
                created_by_user TEXT,
                created_by_device TEXT,
                created_at TEXT,
                sync_status TEXT NOT NULL DEFAULT 'local_only',
                sync_version INTEGER NOT NULL DEFAULT 1,
                storage_key TEXT
            )");

        CreateDocumentIndices(conn);
    }

    private static void ExecuteSql(SqliteConnection conn, string sql)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.ExecuteNonQuery();
    }

    public static void EnsureAuditDb(string baseDir)
    {
        var dbPath = Path.Combine(baseDir, "workaudit_audit.db");
        using var conn = new SqliteConnection($"Data Source={dbPath}");
        conn.Open();

        ExecuteSql(conn, @"
            CREATE TABLE IF NOT EXISTS audit_log (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                uuid TEXT UNIQUE NOT NULL,
                timestamp TEXT NOT NULL,
                actor TEXT NOT NULL,
                action TEXT NOT NULL,
                entity_type TEXT NOT NULL,
                entity_uuid TEXT,
                entity_id INTEGER,
                details TEXT,
                prev_hash TEXT NOT NULL,
                sync_status TEXT NOT NULL DEFAULT 'local_only'
            )");

        ExecuteSql(conn, "CREATE INDEX IF NOT EXISTS idx_audit_log_timestamp ON audit_log(timestamp)");
        ExecuteSql(conn, "CREATE INDEX IF NOT EXISTS idx_audit_log_entity_uuid ON audit_log(entity_uuid)");
        ExecuteSql(conn, "CREATE INDEX IF NOT EXISTS idx_audit_log_entity_type ON audit_log(entity_type)");
    }

    private static void CreateDocumentIndices(SqliteConnection conn)
    {
        ExecuteSql(conn, "CREATE INDEX IF NOT EXISTS idx_documents_uuid ON documents(uuid)");
        ExecuteSql(conn, "CREATE INDEX IF NOT EXISTS idx_documents_file_path ON documents(file_path)");
        ExecuteSql(conn, "CREATE INDEX IF NOT EXISTS idx_documents_section ON documents(section)");
        ExecuteSql(conn, "CREATE INDEX IF NOT EXISTS idx_documents_status ON documents(status)");
        ExecuteSql(conn, "CREATE INDEX IF NOT EXISTS idx_documents_capture_time ON documents(capture_time)");
        ExecuteSql(conn, "CREATE INDEX IF NOT EXISTS idx_documents_sync_status ON documents(sync_status)");
        ExecuteSql(conn, "CREATE INDEX IF NOT EXISTS idx_documents_engagement ON documents(engagement)");
        ExecuteSql(conn, "CREATE INDEX IF NOT EXISTS idx_documents_ocr_text ON documents(ocr_text)");
    }
}
