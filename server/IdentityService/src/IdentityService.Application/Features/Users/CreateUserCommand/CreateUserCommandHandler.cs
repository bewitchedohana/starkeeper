using System.Data;
using FluentResults;
using IdentityService.Domain.Users.Entities;
using Mediator;
using Microsoft.AspNetCore.Identity;

namespace IdentityService.Application.Features.Users.CreateUserCommand;

internal sealed class CreateUserCommandHandler(UserManager<ApplicationUser> userManager) : IRequestHandler<CreateUserCommand, Result<Guid>>
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    public async ValueTask<Result<Guid>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        ApplicationUser user = ApplicationUser.Create(request.Email);
        IdentityResult result = await _userManager.CreateAsync(user, request.Password);

        return result.Succeeded ? Result.Ok(user.Id) : Result.Fail(result.Errors.Select(e => e.Description));
    }
}
