using Application.DTOs;
using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Domain.Exceptions;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using Xunit;

namespace Application.Tests;

public class AuthServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock;
    private readonly Mock<IValidator<RegisterRequestDto>> _registerValidatorMock;
    private readonly Mock<IValidator<LoginRequestDto>> _loginValidatorMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _jwtTokenGeneratorMock = new Mock<IJwtTokenGenerator>();
        _registerValidatorMock = new Mock<IValidator<RegisterRequestDto>>();
        _loginValidatorMock = new Mock<IValidator<LoginRequestDto>>();

        _authService = new AuthService(
            _unitOfWorkMock.Object,
            _passwordHasherMock.Object,
            _jwtTokenGeneratorMock.Object,
            _registerValidatorMock.Object,
            _loginValidatorMock.Object);
    }

    [Fact]
    public async Task RegisterAsync_ValidRequest_ReturnsAuthResponseDto()
    {
        // Arrange
        var request = new RegisterRequestDto { Username = "user1", Email = "user1@example.com", Password = "Password123" };
        var refreshToken = new RefreshToken { Token = "ref-123", ExpiresOn = DateTime.UtcNow.AddDays(7) };

        _registerValidatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _unitOfWorkMock.Setup(x => x.Users.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _unitOfWorkMock.Setup(x => x.Users.GetByUsernameAsync(request.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _passwordHasherMock.Setup(x => x.HashPassword(request.Password)).Returns("hashed_password");
        _jwtTokenGeneratorMock.Setup(x => x.GenerateAccessToken(It.IsAny<User>())).Returns("jwt_access_token");
        _jwtTokenGeneratorMock.Setup(x => x.GenerateRefreshToken(It.IsAny<int>())).Returns(refreshToken);

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("jwt_access_token");
        result.RefreshToken.Should().Be("ref-123");
        result.Email.Should().Be("user1@example.com");
    }

    [Fact]
    public async Task LoginAsync_InvalidPassword_ThrowsUnauthorizedDomainException()
    {
        // Arrange
        var request = new LoginRequestDto { Email = "user1@example.com", Password = "WrongPassword" };
        var user = new User { Id = 1, Email = request.Email, PasswordHash = "hashed_password" };

        _loginValidatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _unitOfWorkMock.Setup(x => x.Users.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasherMock.Setup(x => x.VerifyPassword(request.Password, user.PasswordHash))
            .Returns(false);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedDomainException>(() => _authService.LoginAsync(request));
    }
}
