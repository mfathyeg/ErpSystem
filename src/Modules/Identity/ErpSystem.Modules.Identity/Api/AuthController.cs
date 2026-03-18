using ErpSystem.Modules.Identity.Models;
using ErpSystem.Modules.Identity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ErpSystem.Modules.Identity.Api;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly JwtSettings _jwtSettings;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtTokenService jwtTokenService,
        IOptions<JwtSettings> jwtSettings)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtTokenService = jwtTokenService;
        _jwtSettings = jwtSettings.Value;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _userManager.FindByNameAsync(request.Username)
                   ?? await _userManager.FindByEmailAsync(request.Username);

        if (user == null || !user.IsActive)
        {
            return Unauthorized(new { message = "بيانات الدخول غير صحيحة" });
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            if (result.IsLockedOut)
            {
                return Unauthorized(new { message = "تم قفل الحساب. يرجى المحاولة لاحقاً" });
            }
            return Unauthorized(new { message = "بيانات الدخول غير صحيحة" });
        }

        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _jwtTokenService.GenerateAccessToken(user, roles);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationInDays);
        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        return Ok(new LoginResponse(
            accessToken,
            refreshToken,
            new UserDto(
                user.Id,
                user.UserName!,
                user.Email!,
                user.FirstName,
                user.LastName,
                roles.FirstOrDefault() ?? "Employee",
                user.IsActive,
                user.CreatedAt),
            DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes)));
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var existingUser = await _userManager.FindByNameAsync(request.Username)
                          ?? await _userManager.FindByEmailAsync(request.Email);

        if (existingUser != null)
        {
            return BadRequest(new { message = "اسم المستخدم أو البريد الإلكتروني مسجل مسبقاً" });
        }

        var user = new ApplicationUser
        {
            UserName = request.Username,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            return BadRequest(new { message = string.Join(", ", result.Errors.Select(e => e.Description)) });
        }

        await _userManager.AddToRoleAsync(user, "Employee");

        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _jwtTokenService.GenerateAccessToken(user, roles);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationInDays);
        await _userManager.UpdateAsync(user);

        return Ok(new LoginResponse(
            accessToken,
            refreshToken,
            new UserDto(
                user.Id,
                user.UserName!,
                user.Email!,
                user.FirstName,
                user.LastName,
                roles.FirstOrDefault() ?? "Employee",
                user.IsActive,
                user.CreatedAt),
            DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes)));
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var user = _userManager.Users.FirstOrDefault(u => u.RefreshToken == request.RefreshToken);

        if (user == null || !_jwtTokenService.ValidateRefreshToken(user, request.RefreshToken))
        {
            return Unauthorized(new { message = "رمز التحديث غير صالح" });
        }

        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _jwtTokenService.GenerateAccessToken(user, roles);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationInDays);
        await _userManager.UpdateAsync(user);

        return Ok(new LoginResponse(
            accessToken,
            refreshToken,
            new UserDto(
                user.Id,
                user.UserName!,
                user.Email!,
                user.FirstName,
                user.LastName,
                roles.FirstOrDefault() ?? "Employee",
                user.IsActive,
                user.CreatedAt),
            DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes)));
    }

    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId != null && Guid.TryParse(userId, out var guid))
        {
            var user = await _userManager.FindByIdAsync(guid.ToString());
            if (user != null)
            {
                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = null;
                await _userManager.UpdateAsync(user);
            }
        }
        return Ok(new { message = "تم تسجيل الخروج بنجاح" });
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId == null || !Guid.TryParse(userId, out var guid))
        {
            return Unauthorized();
        }

        var user = await _userManager.FindByIdAsync(guid.ToString());
        if (user == null)
        {
            return Unauthorized();
        }

        var roles = await _userManager.GetRolesAsync(user);

        return Ok(new UserDto(
            user.Id,
            user.UserName!,
            user.Email!,
            user.FirstName,
            user.LastName,
            roles.FirstOrDefault() ?? "Employee",
            user.IsActive,
            user.CreatedAt));
    }
}

public record LoginRequest(string Username, string Password);
public record RegisterRequest(string Username, string Email, string Password, string FirstName, string LastName);
public record RefreshTokenRequest(string RefreshToken);
public record LoginResponse(string Token, string RefreshToken, UserDto User, DateTime ExpiresAt);
public record UserDto(Guid Id, string Username, string Email, string FirstName, string LastName, string Role, bool IsActive, DateTime CreatedAt);
