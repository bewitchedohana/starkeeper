using IdentityService.Domain.Exceptions;
using IdentityService.Domain.Users.Entities;

namespace IdentityService.Domain.Users.Events;

public sealed class UserCreatedDomainEvent
{
    public Guid UserId { get; private set; }
    public string UserEmail { get; private set; } = string.Empty;
    private UserCreatedDomainEvent() { }

    public static UserCreatedDomainEvent Create(ApplicationUser user)
    {
        DomainException.ThrowWhen(string.IsNullOrWhiteSpace(user.Email), "E-mail cannot be null");
        return new()
        {
            UserId = user.Id,
            UserEmail = user.Email!
        };
    }
}
