using FluentValidation;
using Identity.Application.Interfaces;
using Identity.Domain.Common;
using MediatR;

namespace Identity.Application.Features.Users.Commands.UpdateUserRoles;

public class UpdateUserRolesCommand : IRequest<Result>
{
    public Guid UserId { get; init; }
    public List<string> Roles { get; init; } = new();
}

public class UpdateUserRolesCommandValidator : AbstractValidator<UpdateUserRolesCommand>
{
    public UpdateUserRolesCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");
        
        RuleFor(x => x.Roles)
            .NotEmpty().WithMessage("At least one role is required");
    }
}

public class UpdateUserRolesCommandHandler : IRequestHandler<UpdateUserRolesCommand, Result>
{
    private readonly IIdentityService _identityService;
    
    public UpdateUserRolesCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }
    
    public async Task<Result> Handle(
        UpdateUserRolesCommand request, 
        CancellationToken cancellationToken)
    {
        return await _identityService.UpdateUserRolesAsync(request.UserId, request.Roles);
    }
}