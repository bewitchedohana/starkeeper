using Bogus;
using IdentityService.Application.Features.Users.CreateUserCommand;
using IdentityService.Domain.Users.Entities;
using Microsoft.AspNetCore.Identity;
using Moq;
using Shouldly;

namespace IdentityService.Application.Tests.Unit;

public class CreateUserCommandHandlerTests
{
    private readonly Faker _faker = new();
    private readonly Mock<IUserStore<ApplicationUser>> _userStoreMock = new();
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly CreateUserCommandHandler _sut;

    public CreateUserCommandHandlerTests()
    {
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            _userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _sut = new(_userManagerMock.Object);
    }



    [Fact(DisplayName = "Should return error when user creation fails due to invalid email")] 
    public async Task Handle_InvalidEmail_ReturnsIdentityError()
    {
        // Arrange
        var command = new CreateUserCommand("invalid-email", "ValidPassword123!");
        var identityErrors = new[] { new IdentityError { Code = "Email", Description = "Invalid email" } };
        _userManagerMock.Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), command.Password))
            .ReturnsAsync(IdentityResult.Failed(identityErrors));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Message == "Invalid email");
    }



    [Fact(DisplayName = "Should return error when user creation fails due to duplicate email")]
    public async Task Handle_DuplicateEmail_ReturnsIdentityError()
    {
        // Arrange
        var command = new CreateUserCommand(_faker.Internet.Email(), _faker.Internet.Password());
        var identityErrors = new[] { new IdentityError { Code = "DuplicateEmail", Description = "Email already exists" } };
        _userManagerMock.Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), command.Password))
            .ReturnsAsync(IdentityResult.Failed(identityErrors));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Message == "Email already exists");
    }



    [Fact(DisplayName = "Should return error when user creation fails due to weak password")]
    public async Task Handle_WeakPassword_ReturnsIdentityError()
    {
        // Arrange
        var command = new CreateUserCommand(_faker.Internet.Email(), "weak");
        var identityErrors = new[] { new IdentityError { Code = "Password", Description = "Password is too weak" } };
        _userManagerMock.Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), command.Password))
            .ReturnsAsync(IdentityResult.Failed(identityErrors));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Message == "Password is too weak");
    }



    [Fact(DisplayName = "Should return error when user creation fails due to short password")]
    public async Task Handle_ShortPassword_ReturnsIdentityError()
    {
        // Arrange
        var command = new CreateUserCommand(_faker.Internet.Email(), "Short1");
        var identityErrors = new[] { new IdentityError { Code = "Password", Description = "Password is too short" } };
        _userManagerMock.Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), command.Password))
            .ReturnsAsync(IdentityResult.Failed(identityErrors));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Message == "Password is too short");
    }



    [Fact(DisplayName = "Should return error when user creation fails due to password missing non-alphanumeric character")]
    public async Task Handle_PasswordMissingNonAlphanumeric_ReturnsIdentityError()
    {
        // Arrange
        var command = new CreateUserCommand(_faker.Internet.Email(), "NoSpecialChar123");
        var identityErrors = new[] { new IdentityError { Code = "Password", Description = "Password must contain a non-alphanumeric character" } };
        _userManagerMock.Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), command.Password))
            .ReturnsAsync(IdentityResult.Failed(identityErrors));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Message == "Password must contain a non-alphanumeric character");
    }



    [Fact(DisplayName = "Should return error when user creation fails due to password missing lowercase letter")]
    public async Task Handle_PasswordMissingLowercase_ReturnsIdentityError()
    {
        // Arrange
        var command = new CreateUserCommand(_faker.Internet.Email(), "ALLUPPERCASE123!");
        var identityErrors = new[] { new IdentityError { Code = "Password", Description = "Password must contain a lowercase letter" } };
        _userManagerMock.Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), command.Password))
            .ReturnsAsync(IdentityResult.Failed(identityErrors));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Message == "Password must contain a lowercase letter");
    }



    [Fact(DisplayName = "Should return error when user creation fails due to password missing uppercase letter")]
    public async Task Handle_PasswordMissingUppercase_ReturnsIdentityError()
    {
        // Arrange
        var command = new CreateUserCommand(_faker.Internet.Email(), "alllowercase123!");
        var identityErrors = new[] { new IdentityError { Code = "Password", Description = "Password must contain an uppercase letter" } };
        _userManagerMock.Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), command.Password))
            .ReturnsAsync(IdentityResult.Failed(identityErrors));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Message == "Password must contain an uppercase letter");
    }



    [Fact(DisplayName = "Should return GUID when user is successfully created")]
    public async Task Handle_ValidUser_ReturnsGuid()
    {
        // Arrange
        var command = new CreateUserCommand(_faker.Internet.Email(), "@ValidPassword123!");
        var userId = Guid.NewGuid();
        _userManagerMock.Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), command.Password))
            .ReturnsAsync(IdentityResult.Success)
            .Callback<ApplicationUser, string>((user, _) => user.Id = userId);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(userId);
    }


    [Fact(DisplayName = "Should return error with identity errors when user creation fails generically")]
    public async Task Handle_UserManagerFailsToCreateUser_ReturnsIdentityErrors()
    {
        // Arrange
        var command = new CreateUserCommand(_faker.Internet.Email(), "@ValidPassword123!");
        var identityErrors = new[]
        {
            new IdentityError { Code = "1", Description = "Error 1" },
            new IdentityError { Code = "2", Description = "Error 2" },
        };
        _userManagerMock.Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), command.Password))
            .ReturnsAsync(IdentityResult.Failed(identityErrors));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.Count.ShouldBe(identityErrors.Length);
        foreach (var identityError in identityErrors)
        {
            result.Errors.ShouldContain(e => e.Message == identityError.Description);
        }
    }
}
