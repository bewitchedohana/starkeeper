using System.Text.RegularExpressions;
using FluentValidation;
using IdentityService.Domain.Errors;
using IdentityService.Domain.Users.Entities;
using Microsoft.AspNetCore.Identity;

namespace IdentityService.Application.Features.Users.CreateUserCommand;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public CreateUserCommandValidator(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;

        RuleFor(c => c.Email)
            .NotNull()
            .NotEmpty()
            .EmailAddress()
            .MustAsync(EmailIsAvailable)
            .WithMessage(AccountErrors.InvalidEmailAddress.Message);

        RuleFor(c => c.Password)
            .NotNull()
            .NotEmpty()
            .MinimumLength(16)
            .Matches(new Regex(@"\d"))
            .WithMessage(AccountErrors.WeakPassword.Message);
    }

    private async Task<bool> EmailIsAvailable(string email, CancellationToken token)
        => await _userManager.FindByEmailAsync(email ?? string.Empty) is null;
}
