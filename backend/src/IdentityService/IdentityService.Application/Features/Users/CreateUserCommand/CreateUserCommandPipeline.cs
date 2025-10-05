using FluentResults;
using FluentValidation.Results;
using IdentityService.Domain.Users.Entities;
using Mediator;
using Microsoft.AspNetCore.Identity;

namespace IdentityService.Application.Features.Users.CreateUserCommand;

internal sealed class CreateUserCommandPipeline(UserManager<ApplicationUser> userManager)
        : IPipelineBehavior<CreateUserCommand, Result<Guid>>
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    public async ValueTask<Result<Guid>> Handle(CreateUserCommand message, MessageHandlerDelegate<CreateUserCommand, Result<Guid>> next, CancellationToken cancellationToken)
    {
        CreateUserCommandValidator validator = new(_userManager);
        ValidationResult validationResult = await validator.ValidateAsync(message, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Fail(validationResult.Errors.Select(e => e.ErrorMessage));
        }

        var result = await next(message, cancellationToken);
        return result;
    }
}
