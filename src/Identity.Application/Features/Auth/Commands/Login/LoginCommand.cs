using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using Identity.Application.DTOs;
using Identity.Application.Interfaces;
using Identity.Domain.Common;
using Identity.Domain.Entities;
using MediatR;

namespace Identity.Application.Features.Auth.Commands.Login;

public record LoginCommand : IRequest<Result<AuthResponseDto>>
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");
        
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required");
    }
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponseDto>>
{
    private readonly IIdentityService _identityService;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;
    
    public LoginCommandHandler(
        IIdentityService identityService,
        ITokenService tokenService,
        IMapper mapper)
    {
        _identityService = identityService;
        _tokenService = tokenService;
        _mapper = mapper;
    }
    
    public async Task<Result<AuthResponseDto>> Handle(
        LoginCommand request, 
        CancellationToken cancellationToken)
    {
        var user = await _identityService.GetUserByEmailAsync(request.Email);
        if (user == null || !await _identityService.CheckPasswordAsync(user, request.Password))
            return Result.Failure<AuthResponseDto>("Invalid email or password");
        
        if (!user.IsActive)
            return Result.Failure<AuthResponseDto>("Account is deactivated");
        
        var tokenResult = await _tokenService.GenerateTokenAsync(user);
        
        var userDto = _mapper.Map<UserDto>(user);
        userDto.Roles = await _identityService.GetUserRolesAsync(user.Id);
        userDto.Permissions = await _identityService.GetUserPermissionsAsync(user.Id);
        
        return Result.Success(new AuthResponseDto
        {
            AccessToken = tokenResult.AccessToken,
            RefreshToken = tokenResult.RefreshToken,
            ExpiresAt = tokenResult.ExpiresAt,
            User = userDto
        });
    }
}