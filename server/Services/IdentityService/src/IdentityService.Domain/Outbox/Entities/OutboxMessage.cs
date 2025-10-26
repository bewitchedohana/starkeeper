using IdentityService.Domain.Abstractions;
using IdentityService.Domain.Exceptions;
using Newtonsoft.Json;

namespace IdentityService.Domain.Outbox.Entities;

public class OutboxMessage : BaseEntity
{

    public string Content { get; private set; } = string.Empty;
    public DateTime OccuredOn { get; private set; } = DateTime.UtcNow;
    public DateTime? ProcessedOn { get; private set; }
    public string Type { get; private set; } = string.Empty;

    protected OutboxMessage() { }

    public static OutboxMessage Create(string type, string content)
        => new()
        {
            Content = content,
            Type = type
        };

    public static OutboxMessage Create(object obj)
    {
        string? type = obj.GetType().FullName;
        DomainException.ThrowWhen(string.IsNullOrEmpty(type), "Couldn't parse received object");
        string content = JsonConvert.SerializeObject(obj);

        return Create(type!, content);
    }

    public void MarkAsProcessed()
        => ProcessedOn = DateTime.UtcNow;
}
