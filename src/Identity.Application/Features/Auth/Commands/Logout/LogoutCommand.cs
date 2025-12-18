using System;
using System.Threading;
using System.Threading.Tasks;
using Identity.Application.Interfaces;
using Identity.Domain.Common;
using MediatR;

namespace Identity.Application.Features.Auth.Commands.Logout;

public record LogoutCommand : IRequest<Result>
{
    public Guid UserId { get; init; }
    public string RefreshToken { get; init; } = string.Empty;
}

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result>
{
    private readonly IIdentityService _identityService;
    
    public LogoutCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }
    
    public async Task<Result> Handle(
        LogoutCommand request, 
        CancellationToken cancellationToken)
    {
        return await _identityService.RevokeRefreshTokenAsync(request.UserId, request.RefreshToken);
    }
}