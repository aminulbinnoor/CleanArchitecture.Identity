using FluentValidation;
using Identity.Application.DTOs;
using Identity.Application.Interfaces;
using Identity.Domain.Common;
using MediatR;

namespace Identity.Application.Features.Roles.Commands.UpdateRole;

public class UpdateRoleCommand : IRequest<Result<RoleDto>>
{
    public Guid RoleId { get; init; }
    public string? Description { get; init; }
}

public class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand>
{
    public UpdateRoleCommandValidator()
    {
        RuleFor(x => x.RoleId)
            .NotEmpty().WithMessage("Role ID is required");
        
        RuleFor(x => x.Description)
            .MaximumLength(500).When(x => !string.IsNullOrEmpty(x.Description))
            .WithMessage("Description cannot exceed 500 characters");
    }
}

public class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand, Result<RoleDto>>
{
    private readonly IRoleService _roleService;
    
    public UpdateRoleCommandHandler(IRoleService roleService)
    {
        _roleService = roleService;
    }
    
    public async Task<Result<RoleDto>> Handle(
        UpdateRoleCommand request, 
        CancellationToken cancellationToken)
    {
        return await _roleService.UpdateRoleAsync(request.RoleId, request.Description);
    }
}