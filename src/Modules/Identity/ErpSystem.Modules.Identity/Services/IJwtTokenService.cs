using ErpSystem.Modules.Identity.Models;

namespace ErpSystem.Modules.Identity.Services;

public interface IJwtTokenService
{
    string GenerateAccessToken(ApplicationUser user, IList<string> roles);
    string GenerateRefreshToken();
    bool ValidateRefreshToken(ApplicationUser user, string refreshToken);
}
