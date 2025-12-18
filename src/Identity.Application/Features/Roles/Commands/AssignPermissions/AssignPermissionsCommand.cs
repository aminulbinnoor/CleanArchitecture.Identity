using FluentValidation;
using Identity.Application.DTOs;
using Identity.Application.Interfaces;
using Identity.Domain.Common;
using MediatR;

namespace Identity.Application.Features.Roles.Commands.AssignPermissions;

public class AssignPermissionsCommand : IRequest<Result<RoleDto>>
{
    public Guid RoleId { get; init; }
    public List<string> Permissions { get; init; } = new();
}

public class AssignPermissionsCommandValidator : AbstractValidator<AssignPermissionsCommand>
{
    public AssignPermissionsCommandValidator()
    {
        RuleFor(x => x.RoleId)
            .NotEmpty().WithMessage("Role ID is required");
        
        RuleFor(x => x.Permissions)
            .NotEmpty().WithMessage("At least one permission is required");
    }
}

public class AssignPermissionsCommandHandler : IRequestHandler<AssignPermissionsCommand, Result<RoleDto>>
{
    private readonly IRoleService _roleService;
    
    public AssignPermissionsCommandHandler(IRoleService roleService)
    {
        _roleService = roleService;
    }
    
    public async Task<Result<RoleDto>> Handle(
        AssignPermissionsCommand request, 
        CancellationToken cancellationToken)
    {
        return await _roleService.AssignPermissionsAsync(request.RoleId, request.Permissions);
    }
}