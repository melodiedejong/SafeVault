using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SafeVault.Core;
using SafeVault.Data.Repositories;
using SafeVault.Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace SafeVault.Web.Services
{
  public class AuthService : IAuthService
  {
    private readonly IUserRepository _userRepo;
    private readonly IPasswordHasher<User> _hasher;
    private readonly IConfiguration _config;

    public AuthService(
      IUserRepository userRepo,
      IPasswordHasher<User> hasher,
      IConfiguration config)
    {
      _userRepo = userRepo;
      _hasher   = hasher;
      _config   = config;
    }

    public async Task<string?> GenerateTokenAsync(string username, string password)
    {
      var user = await _userRepo.GetByUsernameAsync(username);
      if (user == null) return null;

      // Check for lockout
      if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow)
      {
        return null; // User is locked out
      }

      bool isValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

      if (!isValid)
      {
        user.FailedLoginAttempts++;
        if (user.FailedLoginAttempts >= 5)
        {
          user.LockoutEnd = DateTime.UtcNow.AddMinutes(15); // Lock for 15 minutes
        }
        await _userRepo.SaveChangesAsync();
        return null;
      }

      // Reset lockout info on successful login
      user.FailedLoginAttempts = 0;
      user.LockoutEnd = null;
      await _userRepo.SaveChangesAsync();

      // Create claims
      var claims = new List<Claim>
      {
        new Claim(ClaimTypes.Name, user.Username),
        new Claim(ClaimTypes.Role, user.Role)
      };

      // Build token
      var jwt = _config.GetSection("Jwt");
      var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["SecretKey"]!));
      var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

      var token = new JwtSecurityToken(
        issuer:            jwt["Issuer"],
        audience:          jwt["Audience"],
        claims:            claims,
        expires:           DateTime.UtcNow.AddHours(1),
        signingCredentials: creds);

      return new JwtSecurityTokenHandler().WriteToken(token);
    }
  }
}
