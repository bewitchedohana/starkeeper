using System;
using IdentityService.Domain.Users.Events;
using IdentityService.Infrastructure.Sagas.Users;
using Microsoft.Extensions.Logging;
using Polly.Fallback;
using Rebus.Bus;
using Rebus.Handlers;
using Rebus.Messages;
using Rebus.Sagas;
using Shared.Events.Integration;

namespace IdentityService.Infrastructure.Sagas;

public class UserRegistrationSaga : Saga<UserRegistrationSagaData>,
    IAmInitiatedBy<UserCreatedDomainEvent>,
    IHandleMessages<UserEmailConfirmedEvent>
{
    private readonly IBus _bus;
    private readonly ILogger<UserRegistrationSaga> _logger;

    public UserRegistrationSaga(IBus bus, ILogger<UserRegistrationSaga> logger)
    {
        _bus = bus;
        _logger = logger;
    }

    public async Task Handle(UserCreatedDomainEvent message)
    {
        _logger.LogInformation("Initiated registration for {UserId}", message.UserId);
        Data.Email = message.UserEmail;
        Data.UserId = message.UserId;
        Data.StartedAt = DateTime.UtcNow;

        await _bus.Publish(new UserRegistrationStartedEvent
        {
            UserEmail = message.UserEmail,
            UserId = message.UserId
        });

        await _bus.DeferLocal(
            TimeSpan.FromMinutes(5),
            new UserRegistrationTimeoutMessage { UserId = Data.UserId }
        );
    }

    public async Task Handle(UserRegistrationTimeoutMessage timeoutMessage)
    {
        if (Data.CompletedAt.HasValue)
        {
            return;
        }

        if (Data.RetryCount >= 3)
        {
            MarkAsComplete();
        }

        Data.RetryCount++;
        _logger.LogInformation("Retrying registration of user {UserId}", Data.UserId);
        await _bus.Publish(new UserRegistrationStartedEvent
        {
            UserEmail = Data.Email,
            UserId = Data.UserId
        });

        await _bus.DeferLocal(TimeSpan.FromMinutes(5), new UserRegistrationTimeoutMessage { UserId = Data.UserId });
    }

    public Task Handle(UserEmailConfirmedEvent message)
    {
        Data.CompletedAt = DateTime.UtcNow;
        MarkAsComplete();
        return Task.CompletedTask;
    }

    protected override void CorrelateMessages(ICorrelationConfig<UserRegistrationSagaData> config)
    {
        config.Correlate<UserCreatedDomainEvent>(m => m.UserId, m => m.UserId);
        config.Correlate<UserEmailConfirmedEvent>(m => m.UserId, m => m.UserId);
    }
}
