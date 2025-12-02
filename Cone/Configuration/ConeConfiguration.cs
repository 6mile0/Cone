namespace Cone.Configuration;

public class ConeConfiguration
{
    public required IReadOnlyList<string> AllowedEmailEndPrefixes { get; init; }
    public required IReadOnlyList<string> EmergencyAdminEmails { get; init; }
}