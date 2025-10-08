namespace IdentityService.Persistence.Options;

public sealed class ConnectionStringOptions
{
    public const string SectionName = "ConnectionStrings";
    public string IdentityServiceContext { get; set; } = string.Empty;
}
