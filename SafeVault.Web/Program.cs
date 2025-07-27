using Microsoft.Extensions.Options;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore; 
using Microsoft.AspNetCore.Authentication.JwtBearer;
using SafeVault.Core;
using SafeVault.Core.Sanitization;
using SafeVault.Data;
using SafeVault.Data.Repositories;
using SafeVault.Web.Services;
using SafeVault.Data.Entities;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Add EF Core DbContext
builder.Services.AddDbContext<SafeVaultContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


// Register repository
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IInputSanitizer, InputSanitizer>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();


var jwtCfg = builder.Configuration.GetSection("Jwt");
var keyBytes = Encoding.UTF8.GetBytes(jwtCfg["SecretKey"]!);

builder.Services
  .AddAuthentication(options =>
  {
      options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
      options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    
  })
  .AddJwtBearer(options =>
  {
      options.TokenValidationParameters = new TokenValidationParameters
      {
          ValidateIssuer = true,
          ValidateAudience = true,
          ValidateLifetime = true,
          ValidateIssuerSigningKey = true,
          ValidIssuer = jwtCfg["Issuer"],
          ValidAudience = jwtCfg["Audience"],
          IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
          RoleClaimType  = ClaimTypes.Role 
      };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var token = context.Request.Cookies["JwtToken"];
            if (!string.IsNullOrEmpty(token))
            {
                context.Token = token;
            }
            return Task.CompletedTask;
        }
    };

  });

builder.Services.AddAuthorization();

// Add controllers
builder.Services.AddControllers();

//Add Razor Pages
builder.Services.AddRazorPages();

var app = builder.Build();

app.UseStatusCodePages(async context =>
{
    if (context.HttpContext.Response.StatusCode == 403)
    {
        context.HttpContext.Response.Redirect("/AccessDenied");
    }
});

app.UseRouting();


app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();

app.Run();
