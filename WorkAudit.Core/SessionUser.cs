namespace WorkAudit.Core;

/// <summary>
/// Session user identity (name, staff ID) - stored in memory for the session only.
/// </summary>
public class SessionUser
{
    public string Name { get; set; } = string.Empty;
    public string StaffId { get; set; } = string.Empty;
}
