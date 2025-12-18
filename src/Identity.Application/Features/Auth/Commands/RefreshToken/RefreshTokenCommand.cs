using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Identity.Application.DTOs;
using Identity.Application.Interfaces;
using Identity.Domain.Common;
using Identity.Domain.Entities;
using MediatR;

namespace Identity.Application.Features.Auth.Commands.RefreshToken;

public record RefreshTokenCommand : IRequest<Result<AuthResponseDto>>
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
}

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<AuthResponseDto>>
{
    private readonly ITokenService _tokenService;
    private readonly IIdentityService _identityService;
    private readonly IMapper _mapper;
    
    public RefreshTokenCommandHandler(
        ITokenService tokenService,
        IIdentityService identityService,
        IMapper mapper)
    {
        _tokenService = tokenService;
        _identityService = identityService;
        _mapper = mapper;
    }
    
    public async Task<Result<AuthResponseDto>> Handle(
        RefreshTokenCommand request, 
        CancellationToken cancellationToken)
    {
        var principal = _tokenService.GetPrincipalFromExpiredToken(request.AccessToken);
        if (principal == null)
            return Result.Failure<AuthResponseDto>("Invalid token");
        
        var userIdClaim = principal.FindFirst("sub")?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
            return Result.Failure<AuthResponseDto>("Invalid token");
        
        var refreshToken = await _identityService.GetRefreshTokenAsync(request.RefreshToken);
        if (refreshToken == null || refreshToken.UserId != userId || !refreshToken.IsActive)
            return Result.Failure<AuthResponseDto>("Invalid refresh token");
        
        var user = await _identityService.GetUserByIdAsync(userId);
        if (user == null)
            return Result.Failure<AuthResponseDto>("User not found");
        
        // Revoke old refresh token
        await _identityService.RevokeRefreshTokenAsync(userId, request.RefreshToken);
        
        // Generate new tokens
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