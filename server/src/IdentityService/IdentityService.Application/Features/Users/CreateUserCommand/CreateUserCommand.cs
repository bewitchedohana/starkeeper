using FluentResults;
using Mediator;

namespace IdentityService.Application.Features.Users.CreateUserCommand;

public sealed record CreateUserCommand(string Email,
    string Password)
    : IRequest<Result<Guid>>;
