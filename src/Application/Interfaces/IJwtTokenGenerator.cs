using Domain.Entities;

namespace Application.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateAccessToken(User user);
    RefreshToken GenerateRefreshToken(int userId);
}
