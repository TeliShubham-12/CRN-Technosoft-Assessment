using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Exceptions;
using FluentValidation;

namespace Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IValidator<RegisterRequestDto> _registerValidator;
    private readonly IValidator<LoginRequestDto> _loginValidator;

    public AuthService(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IValidator<RegisterRequestDto> registerValidator,
        IValidator<LoginRequestDto> loginValidator)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _registerValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new Domain.Exceptions.ValidationException(validationResult.ToDictionary());
        }

        var existingUserByEmail = await _unitOfWork.Users.GetByEmailAsync(request.Email, cancellationToken);
        if (existingUserByEmail != null)
        {
            throw new UnauthorizedDomainException("A user with this email already exists.");
        }

        var existingUserByUsername = await _unitOfWork.Users.GetByUsernameAsync(request.Username, cancellationToken);
        if (existingUserByUsername != null)
        {
            throw new UnauthorizedDomainException("A user with this username already exists.");
        }

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            Role = "User"
        };

        await _unitOfWork.Users.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user);
        var refreshToken = _jwtTokenGenerator.GenerateRefreshToken(user.Id);

        user.RefreshTokens.Add(refreshToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            RefreshTokenExpiresOn = refreshToken.ExpiresOn,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _loginValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new Domain.Exceptions.ValidationException(validationResult.ToDictionary());
        }

        var user = await _unitOfWork.Users.GetByEmailAsync(request.Email, cancellationToken);
        if (user == null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedDomainException("Invalid email or password.");
        }

        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user);
        var refreshToken = _jwtTokenGenerator.GenerateRefreshToken(user.Id);

        user.RefreshTokens.Add(refreshToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            RefreshTokenExpiresOn = refreshToken.ExpiresOn,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role
        };
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            throw new UnauthorizedDomainException("Refresh token is required.");
        }

        var user = await _unitOfWork.Users.GetByRefreshTokenAsync(request.RefreshToken, cancellationToken);
        if (user == null)
        {
            throw new UnauthorizedDomainException("Invalid refresh token.");
        }

        var existingToken = user.RefreshTokens.FirstOrDefault(x => x.Token == request.RefreshToken);
        if (existingToken == null || !existingToken.IsActive)
        {
            throw new UnauthorizedDomainException("Refresh token is expired or revoked.");
        }

        // Revoke current refresh token (Refresh token rotation strategy)
        existingToken.RevokedOn = DateTime.UtcNow;

        var newAccessToken = _jwtTokenGenerator.GenerateAccessToken(user);
        var newRefreshToken = _jwtTokenGenerator.GenerateRefreshToken(user.Id);

        user.RefreshTokens.Add(newRefreshToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken.Token,
            RefreshTokenExpiresOn = newRefreshToken.ExpiresOn,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role
        };
    }

    public async Task RevokeTokenAsync(RevokeTokenRequestDto request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            throw new UnauthorizedDomainException("Refresh token is required.");
        }

        var user = await _unitOfWork.Users.GetByRefreshTokenAsync(request.RefreshToken, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException("RefreshToken", request.RefreshToken);
        }

        var existingToken = user.RefreshTokens.FirstOrDefault(x => x.Token == request.RefreshToken);
        if (existingToken != null && existingToken.IsActive)
        {
            existingToken.RevokedOn = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
