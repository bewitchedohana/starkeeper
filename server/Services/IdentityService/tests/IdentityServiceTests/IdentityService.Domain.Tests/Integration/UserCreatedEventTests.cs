using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Bogus;
using IdentityService.Domain.Abstractions;
using IdentityService.Domain.Outbox.Entities;
using IdentityService.Domain.Users.Entities;
using IdentityService.Domain.Users.Events;
using IdentityService.Persistence.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Shouldly;

namespace IdentityService.Domain.Tests.Integration;

public class UserCreatedEventTests : IntegrationTestBase
{
    private readonly Faker _faker = new();
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationContext _database;

    public UserCreatedEventTests(IntegrationTestApplicationFactory factory) : base(factory)
    {
        var provider = ServiceScope.ServiceProvider;
        _userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
        _database = provider.GetRequiredService<ApplicationContext>();
    }

    [Fact]
    [DisplayName("When user is stored it should store an outbox message")]
    internal async Task ApplicationUser_WhenPersisted_ShouldStoreOutboxMessage()
    {
        // Arrange
        string email = _faker.Internet.Email();
        string password = _faker.Internet.Password();
        ApplicationUser user = ApplicationUser.Create(email);

        // Act
        await _userManager.CreateAsync(user, password);

        // Assert
        OutboxMessage? outboxMessage = _database.OutboxMessages
            .Where(t => t.Type.Equals(nameof(UserCreatedDomainEvent)))
            .FirstOrDefault();

        outboxMessage.ShouldNotBe(null);
        string.IsNullOrEmpty(outboxMessage!.Content).ShouldBe(false);
        UserCreatedDomainEvent? ev = JsonConvert.DeserializeObject(outboxMessage.Content!, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All
        }) as UserCreatedDomainEvent;
        ev.ShouldNotBe(null);
        ev!.UserId.ShouldBe(user.Id);
        ev!.UserEmail.ShouldBe(user.Email);
    }
}
