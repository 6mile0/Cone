namespace Ice.Configuration;

public class IceConfiguration
{
    public required IReadOnlyList<string> AllowedEmailEndPrefixes { get; init; }
    public required IReadOnlyList<string> EmergencyAdminEmails { get; init; }
}