using Microsoft.AspNetCore.Identity;

namespace IdentityService.Domain.Users.Entities;

public class ApplicationRole : IdentityRole<Guid>
{
    private ApplicationRole() : base() { }
}
