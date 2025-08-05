using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EBOM.Common.Models.Configuration;
using Microsoft.Extensions.Options;

namespace EBOM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IOptions<JwtSettings> jwtSettings, ILogger<AuthController> logger)
    {
        _jwtSettings = jwtSettings.Value;
        _logger = logger;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        // For demo purposes, accept admin@ebom.com / admin123
        if (request.Email == "admin@ebom.com" && request.Password == "admin123")
        {
            var token = GenerateJwtToken("1", request.Email, "Admin User");
            
            return Ok(new AuthResponse
            {
                AccessToken = token,
                RefreshToken = Guid.NewGuid().ToString(), // Simple refresh token for demo
                User = new UserDto
                {
                    Id = 1,
                    UserName = "Admin User",
                    Email = request.Email,
                    Roles = new[] { "Admin" }
                },
                ExpiresIn = _jwtSettings.ExpirationMinutes * 60
            });
        }

        return Unauthorized(new { message = "Invalid email or password" });
    }

    private string GenerateJwtToken(string userId, string email, string name)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Name, name),
                new Claim("roles", "Admin")
            }),
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class AuthResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public UserDto User { get; set; } = new();
    public int ExpiresIn { get; set; }
}

public class UserDto
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string[] Roles { get; set; } = Array.Empty<string>();
}