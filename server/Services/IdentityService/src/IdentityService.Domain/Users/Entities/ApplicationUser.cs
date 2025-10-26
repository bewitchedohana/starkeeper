using System.Collections.ObjectModel;
using IdentityService.Domain.Abstractions;
using IdentityService.Domain.Outbox.Entities;
using Microsoft.AspNetCore.Identity;

namespace IdentityService.Domain.Users.Entities;

public class ApplicationUser : IdentityUser<Guid>, IAggregateRoot
{
    private readonly List<OutboxMessage> _events = [];
    private ApplicationUser() : base() { }

    public static ApplicationUser Create(string email)
        => new() { Email = email, UserName = email };

    public void ClearEvents()
        => _events.Clear();

    public IReadOnlyCollection<OutboxMessage> GetEvents()
        => _events.AsReadOnly();

    public void RaiseEvent(OutboxMessage domainEvent)
        => _events.Add(domainEvent);
}
