namespace SolidCqrsFramework.Replaying;

public record ReplayStatus
{
    public const string InProgress = "InProgress";
    public const string Failed = "Failed";
    public const string Success = "Success";
}
