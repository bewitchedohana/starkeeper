using IdentityService.Domain.Users.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Persistence.Contexts;

public class ApplicationContext(DbContextOptions options) 
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options) { }
