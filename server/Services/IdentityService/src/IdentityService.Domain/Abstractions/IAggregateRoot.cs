using System.Collections.ObjectModel;
using IdentityService.Domain.Outbox.Entities;

namespace IdentityService.Domain.Abstractions;

public interface IAggregateRoot
{
    public IReadOnlyCollection<OutboxMessage> GetEvents();

    public void RaiseEvent(OutboxMessage domainEvent);

    public void ClearEvents();
}