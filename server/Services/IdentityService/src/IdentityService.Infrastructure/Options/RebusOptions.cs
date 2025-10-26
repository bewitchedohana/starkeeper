using System;

namespace IdentityService.Infrastructure.Options;

public class RebusOptions
{
    public const string SectionName = "Rebus";
    public string RabbitMqConnectionString { get; set; } = string.Empty;
    public string UserCreatedQueue { get; set; } = string.Empty;
}
