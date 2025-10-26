using System;
using IdentityService.Domain.Abstractions;
using IdentityService.Domain.Outbox.Entities;
using IdentityService.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace IdentityService.Persistence.Interceptors;

public class OutboxInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
        {
            StoreOutboxMessages(eventData.Context);
        }

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            StoreOutboxMessages(eventData.Context);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void StoreOutboxMessages(DbContext context)
    {
        List<EntityEntry> aggregateRoots = [.. context.ChangeTracker
            .Entries()
            .Where(m => m.Entity.GetType().GetInterface(nameof(IAggregateRoot)) != null)];

        if (aggregateRoots.Count == 0)
        {
            return;
        }

        var events = aggregateRoots.Select(ar => ar.Entity)
            .Select(e => e.GetType().GetMethod(nameof(IAggregateRoot.GetEvents))?.Invoke(e, []))
            .Where(ev => ev != null)
            .Cast<List<IDomainEvent>>()
            .SelectMany(del => del)
            .Select(OutboxMessage.Create)
            .ToList();
    }
}
