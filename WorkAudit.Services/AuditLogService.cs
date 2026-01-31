using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using WorkAudit.Core;

namespace WorkAudit.Services;

/// <summary>
/// Appends audit entries to workaudit_audit.db with hash chain.
/// Policy: forward to JSONL first; if forward fails, do not insert into DB.
/// prev_hash = SHA256 of previous row's (uuid|timestamp|actor|action|entity_type|entity_uuid|entity_id|details|prev_hash).
/// </summary>
public class AuditLogService
{
    private readonly string _auditDbPath;
    private readonly AuditForwarder _forwarder;
    private string _lastHash = "0";

    public AuditLogService(string baseDir)
    {
        _auditDbPath = Path.Combine(baseDir, "workaudit_audit.db");
        _forwarder = new AuditForwarder();
        LoadLastHash();
    }

    public void Append(string actor, string action, string entityType, string? entityUuid, int? entityId, object? details)
    {
        var uuid = Guid.NewGuid().ToString();
        var timestamp = DateTime.UtcNow.ToString("o");
        var detailsJson = details != null ? JsonSerializer.Serialize(details) : null;
        var entry = new AuditEntry
        {
            Uuid = uuid,
            Timestamp = timestamp,
            Actor = actor,
            Action = action,
            EntityType = entityType,
            EntityUuid = entityUuid,
            EntityId = entityId,
            Details = detailsJson,
            PrevHash = _lastHash
        };

        if (!_forwarder.Forward(entry))
            return; // Policy: do not insert if forward fails

        using var conn = new SqliteConnection($"Data Source={_auditDbPath}");
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO audit_log (uuid, timestamp, actor, action, entity_type, entity_uuid, entity_id, details, prev_hash, sync_status)
            VALUES (@uuid, @ts, @actor, @action, @entity_type, @entity_uuid, @entity_id, @details, @prev_hash, 'local_only')";
        cmd.Parameters.AddWithValue("@uuid", uuid);
        cmd.Parameters.AddWithValue("@ts", timestamp);
        cmd.Parameters.AddWithValue("@actor", actor);
        cmd.Parameters.AddWithValue("@action", action);
        cmd.Parameters.AddWithValue("@entity_type", entityType);
        cmd.Parameters.AddWithValue("@entity_uuid", entityUuid ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@entity_id", entityId ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@details", detailsJson ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@prev_hash", _lastHash);
        cmd.ExecuteNonQuery();

        _lastHash = ComputeHash(uuid, timestamp, actor, action, entityType, entityUuid, entityId, detailsJson, _lastHash);
    }

    private void LoadLastHash()
    {
        if (!File.Exists(_auditDbPath)) return;
        try
        {
            using var conn = new SqliteConnection($"Data Source={_auditDbPath}");
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT uuid, timestamp, actor, action, entity_type, entity_uuid, entity_id, details, prev_hash FROM audit_log ORDER BY id DESC LIMIT 1";
            using var r = cmd.ExecuteReader();
            if (r.Read())
            {
                var uuid = r.GetString(0);
                var ts = r.GetString(1);
                var actor = r.GetString(2);
                var action = r.GetString(3);
                var entityType = r.GetString(4);
                var entityUuid = r.IsDBNull(5) ? null : r.GetString(5);
                var entityId = r.IsDBNull(6) ? null : (int?)r.GetInt32(6);
                var details = r.IsDBNull(7) ? null : r.GetString(7);
                var prevHash = r.GetString(8);
                _lastHash = ComputeHash(uuid, ts, actor, action, entityType, entityUuid, entityId, details, prevHash);
            }
        }
        catch { }
    }

    private static string ComputeHash(string uuid, string ts, string actor, string action, string entityType,
        string? entityUuid, int? entityId, string? details, string prevHash)
    {
        var payload = $"{uuid}|{ts}|{actor}|{action}|{entityType}|{entityUuid ?? ""}|{entityId ?? 0}|{details ?? ""}|{prevHash}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
