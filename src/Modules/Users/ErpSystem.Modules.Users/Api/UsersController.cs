using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ErpSystem.Modules.Users.Api;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private static readonly List<UserDto> _users = GenerateMockUsers();

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetUsers(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortBy = "createdAt",
        [FromQuery] string? sortDirection = "desc",
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? role = null,
        [FromQuery] bool? isActive = null)
    {
        var query = _users.AsQueryable();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(u =>
                u.Username.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                u.Email.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                u.FirstName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                u.LastName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrEmpty(role))
        {
            query = query.Where(u => u.Role.Equals(role, StringComparison.OrdinalIgnoreCase));
        }

        if (isActive.HasValue)
        {
            query = query.Where(u => u.IsActive == isActive.Value);
        }

        query = sortBy?.ToLower() switch
        {
            "username" => sortDirection == "asc" ? query.OrderBy(u => u.Username) : query.OrderByDescending(u => u.Username),
            "email" => sortDirection == "asc" ? query.OrderBy(u => u.Email) : query.OrderByDescending(u => u.Email),
            "role" => sortDirection == "asc" ? query.OrderBy(u => u.Role) : query.OrderByDescending(u => u.Role),
            "firstname" => sortDirection == "asc" ? query.OrderBy(u => u.FirstName) : query.OrderByDescending(u => u.FirstName),
            _ => sortDirection == "asc" ? query.OrderBy(u => u.CreatedAt) : query.OrderByDescending(u => u.CreatedAt)
        };

        var totalCount = query.Count();
        var data = query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

        return Ok(new PaginatedResponse<UserDto>
        {
            Data = data,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        });
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetUser(int id)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);
        if (user == null)
            return NotFound(new { message = "المستخدم غير موجود" });

        return Ok(user);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public IActionResult CreateUser([FromBody] CreateUserRequest request)
    {
        var newId = _users.Max(u => u.Id) + 1;
        var user = new UserDto
        {
            Id = newId,
            Username = request.Username,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Role = request.Role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _users.Add(user);
        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult UpdateUser(int id, [FromBody] UpdateUserRequest request)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);
        if (user == null)
            return NotFound(new { message = "المستخدم غير موجود" });

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Email = request.Email;
        user.Role = request.Role;
        user.IsActive = request.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        return Ok(user);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult DeleteUser(int id)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);
        if (user == null)
            return NotFound(new { message = "المستخدم غير موجود" });

        _users.Remove(user);
        return NoContent();
    }

    [HttpPatch("{id:int}/toggle-status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult ToggleUserStatus(int id)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);
        if (user == null)
            return NotFound(new { message = "المستخدم غير موجود" });

        user.IsActive = !user.IsActive;
        user.UpdatedAt = DateTime.UtcNow;
        return Ok(user);
    }

    private static List<UserDto> GenerateMockUsers()
    {
        var roles = new[] { "Admin", "Manager", "Employee", "Viewer" };
        var names = new[]
        {
            ("أحمد", "محمد", "ahmed"),
            ("سارة", "علي", "sara"),
            ("خالد", "عبدالله", "khaled"),
            ("نورة", "سعيد", "noura"),
            ("فهد", "العتيبي", "fahad"),
            ("ريم", "الشمري", "reem"),
            ("محمد", "القحطاني", "mohammed"),
            ("لينا", "الحربي", "lina"),
            ("عبدالرحمن", "السعيد", "abdulrahman"),
            ("هند", "الزهراني", "hind")
        };

        var users = new List<UserDto>();
        var random = new Random(42);

        for (int i = 0; i < names.Length; i++)
        {
            var name = names[i];
            users.Add(new UserDto
            {
                Id = i + 1,
                Username = name.Item3,
                Email = $"{name.Item3}@duralux.sa",
                FirstName = name.Item1,
                LastName = name.Item2,
                Role = roles[i % roles.Length],
                IsActive = random.Next(10) > 1,
                CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 365)),
                LastLogin = DateTime.UtcNow.AddHours(-random.Next(1, 72))
            });
        }

        return users;
    }
}

public class PaginatedResponse<T>
{
    public List<T> Data { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastLogin { get; set; }
}

public record CreateUserRequest(
    string Username,
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string Role);

public record UpdateUserRequest(
    string Email,
    string FirstName,
    string LastName,
    string Role,
    bool IsActive);
