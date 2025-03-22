using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using jekirdekCase.Data;
using jekirdekCase.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace jekirdekCase.Controllers;


    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly CRMDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;


    public AuthController(CRMDbContext context, IConfiguration configuration, ILogger<AuthController> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    // ✅ Kullanıcı giriş yapınca JWT Token üretir
 [HttpPost("login")]
 public async Task<IActionResult> Login([FromBody] LoginRequest request)
 {
     var user = _context.Users.SingleOrDefault(u => u.Username == request.Username);

     if (user == null || user.Password != HashPassword(request.Password))
     {
         return Unauthorized("Geçersiz kullanıcı adı veya şifre.");
     }

     try
     {
         var token = GenerateJwtToken(user);
         return Ok(new { Token = token });
     }
     catch (Exception ex)
     {
         _logger.LogError("JWT Token oluşturma hatası: {Message}", ex.Message);
         return StatusCode(500, "Bir hata oluştu. Lütfen sistem yöneticinize başvurun.");
     }
 }

[HttpPost("generate-jwt-token")]
public string GenerateJwtToken(User user)
{
    var jwtSettings = _configuration.GetSection("JwtSettings");
    var secretKey = jwtSettings["Secret"];

    if (string.IsNullOrEmpty(secretKey))
    {
        _logger.LogError("JWT Secret key is null or empty. Check appsettings.json.");
        throw new Exception("JWT Secret değeri null veya boş. Lütfen appsettings.json dosyanızı kontrol edin.");
    }

    var key = Encoding.UTF8.GetBytes(secretKey);
    var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        }),
        Expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["AccessTokenExpirationMinutes"])),
        Issuer = jwtSettings["Issuer"],
        Audience = jwtSettings["Audience"],
        SigningCredentials = credentials
    };

    var tokenHandler = new JwtSecurityTokenHandler();
    var token = tokenHandler.CreateToken(tokenDescriptor);
    return tokenHandler.WriteToken(token);
}



    // 🔑 JWT Token oluşturma fonksiyonu

    // 🛠 Şifre Hashleme Fonksiyonu (SHA-256)
    private string HashPassword(string password)
    {
        using (var sha256 = System.Security.Cryptography.SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}

// 📌 Giriş request modeli
public class LoginRequest
{
    public string Username { get; set; }
    public string Password { get; set; }
}