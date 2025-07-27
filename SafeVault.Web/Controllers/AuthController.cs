using Microsoft.AspNetCore.Mvc;
using SafeVault.Core;
using SafeVault.Data.Repositories;
using SafeVault.Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace SafeVault.Web.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class AuthController : ControllerBase
  {
    private readonly IAuthService _auth;
    private readonly IUserRepository _userRepository;
    private readonly IInputSanitizer _sanitizer;
    private readonly IPasswordHasher<User> _hasher;

    public AuthController(IAuthService auth, IUserRepository userRepository, IInputSanitizer sanitizer, IPasswordHasher<User> hasher)
    {
      _auth = auth;
      _userRepository = userRepository;
      _sanitizer = sanitizer;
      _hasher = hasher;
    }


    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
      var user = await _userRepository.GetByUsernameAsync(dto.Username);
      if (user != null && user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow)
      {
          var minutes = (int)(user.LockoutEnd.Value - DateTime.UtcNow).TotalMinutes;
          return Unauthorized($"Your account is locked due to multiple failed login attempts. Please try again in {minutes} minute(s).");
      }

      var token = await _auth.GenerateTokenAsync(dto.Username, dto.Password);
      if (token == null) return Unauthorized();

      return Ok(new { token });
    }
    
    [HttpPost("register")]
public async Task<IActionResult> Register([FromBody] RegisterDto dto)
{
    if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Email) ||
        string.IsNullOrWhiteSpace(dto.Password) || string.IsNullOrWhiteSpace(dto.Role))
    {
        return BadRequest("All fields are required.");
    }

    // Enforce strong password policy
    // At least 8 chars, one uppercase, one lowercase, one digit, one special char
    var password = dto.Password;
    bool strong = password.Length >= 8
        && password.Any(char.IsUpper)
        && password.Any(char.IsLower)
        && password.Any(char.IsDigit)
        && password.Any(c => "!@#$%^&*()-_=+[]{}|;:'\",.<>?/`~".Contains(c));
    if (!strong)
    {
        return BadRequest("Password must be at least 8 characters and include uppercase, lowercase, digit, and special character.");
    }

    // Sanitize inputs
    var username = _sanitizer.Sanitize(dto.Username);
    var email = _sanitizer.Sanitize(dto.Email);

    // Create User entity
    var user = new User
    {
        Username = username,
        Email = email,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
        Role = dto.Role
    };

    try
    {
        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();
    }
    catch (InvalidOperationException ex)
    {
        return BadRequest(ex.Message);
    }

    return Ok("Registration successful.");
}

  }

  public record LoginDto(string Username, string Password);
  public record RegisterDto(string Username, string Email, string Password, string Role);

}
