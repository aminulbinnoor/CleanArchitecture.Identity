namespace Identity.Application.Features.Auth.Commands.Register;

public record RegisterCommand : IRequest<Result<AuthResponseDto>>
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name cannot exceed 100 characters");
        
        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters");
        
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");
        
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters");
    }
}

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<AuthResponseDto>>
{
    private readonly IIdentityService _identityService;
    private readonly ITokenService _tokenService;
    
    public RegisterCommandHandler(
        IIdentityService identityService,
        ITokenService tokenService)
    {
        _identityService = identityService;
        _tokenService = tokenService;
    }
    
    public async Task<Result<AuthResponseDto>> Handle(
        RegisterCommand request, 
        CancellationToken cancellationToken)
    {
        var result = await _identityService.RegisterAsync(
            request.FirstName,
            request.LastName,
            request.Email,
            request.Password);
        
        if (!result.IsSuccess)
            return Result.Failure<AuthResponseDto>(result.Error);
        
        var user = await _identityService.GetUserByEmailAsync(request.Email);
        if (user == null)
            return Result.Failure<AuthResponseDto>("User not found after registration");
        
        var tokenResult = await _tokenService.GenerateTokenAsync(user);
        
        return Result.Success(new AuthResponseDto
        {
            AccessToken = tokenResult.AccessToken,
            RefreshToken = tokenResult.RefreshToken,
            ExpiresAt = tokenResult.ExpiresAt,
            User = new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Roles = await _identityService.GetUserRolesAsync(user.Id),
                Permissions = await _identityService.GetUserPermissionsAsync(user.Id)
            }
        });
    }
}