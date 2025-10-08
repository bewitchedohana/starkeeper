using Microsoft.AspNetCore.Identity;

namespace IdentityService.Domain.Users.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    private ApplicationUser() : base() { }

    public static ApplicationUser Create(string email)
        => new() { Email = email, UserName = email };
}
