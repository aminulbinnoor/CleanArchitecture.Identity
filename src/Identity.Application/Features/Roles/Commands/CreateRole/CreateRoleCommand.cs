using FluentValidation;
using Identity.Application.DTOs;
using Identity.Application.Interfaces;
using Identity.Domain.Common;
using Identity.Domain.Entities;
using MediatR;
// using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Features.Roles.Commands.CreateRole;

public class CreateRoleCommand : IRequest<Result<RoleDto>>
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public List<string> Permissions { get; init; } = new();
}

public class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Role name is required")
            .MaximumLength(100).WithMessage("Role name cannot exceed 100 characters")
            .Matches("^[a-zA-Z0-9 ]+$").WithMessage("Role name can only contain letters, numbers and spaces");
    }
}

public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, Result<RoleDto>>
{
    private readonly IRoleService _roleService;
    private readonly ILogger<CreateRoleCommandHandler> _logger;
    
    public CreateRoleCommandHandler(
        IRoleService roleService,
        ILogger<CreateRoleCommandHandler> logger)
    {
        _roleService = roleService;
        _logger = logger;
    }
    
    public async Task<Result<RoleDto>> Handle(
        CreateRoleCommand request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating role: {RoleName}", request.Name);
        
        var result = await _roleService.CreateRoleAsync(
            request.Name, 
            request.Description, 
            request.Permissions);
        
        return result;
    }
}