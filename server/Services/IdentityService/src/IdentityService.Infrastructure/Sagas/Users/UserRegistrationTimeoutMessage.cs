using System;

namespace IdentityService.Infrastructure.Sagas.Users;

public class UserRegistrationTimeoutMessage
{
    public Guid UserId { get; set; }
}
