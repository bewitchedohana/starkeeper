using System;
using Rebus.Sagas;

namespace IdentityService.Infrastructure.Sagas.Users;

public class UserRegistrationSagaData : ISagaData
{
    public Guid Id { get; set; }
    public int Revision { get; set; }
    public Guid UserId { get; set; }
    public string Email { get; set; } = default!;
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int RetryCount { get; set; }
}
