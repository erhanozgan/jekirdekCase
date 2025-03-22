using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using jekirdekCase.Data;
using jekirdekCase.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;

namespace jekirdekCase.Controllers
{
    [Route("api/users")]
    [ApiController]
    [Authorize] // ✅ Yetkilendirme Aktif
    public class UserController : ControllerBase
    {
        private readonly CRMDbContext _context;
        private readonly ILogger<UserController> _logger;
        private readonly IConfiguration _configuration; // ✅ JWT için IConfiguration eklendi

        public UserController(CRMDbContext context, ILogger<UserController> logger, IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// ✅ Kullanıcı Girişi (JWT Token Döndürür)
        /// </summary>
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            _logger.LogInformation("Kullanıcı giriş yapmaya çalışıyor: {Username}", request.Username);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
            if (user == null || user.Password != HashPassword(request.Password))
            {
                _logger.LogWarning("Geçersiz giriş denemesi: {Username}", request.Username);
                return Unauthorized(new { message = "Kullanıcı adı veya şifre hatalı!" });
            }

            var token = GenerateJwtToken(user); // ✅ Token oluştur

            _logger.LogInformation("Kullanıcı giriş yaptı: {Username}", request.Username);
            return Ok(new { Token = token });
        }

        /// <summary>
        /// ✅ Tüm kullanıcıları getir (Yetkilendirme Gerektirir)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users.ToListAsync();
            return Ok(users);
        }

        /// <summary>
        /// ✅ Belirli bir kullanıcıyı getir
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { message = "Kullanıcı bulunamadı." });

            return Ok(user);
        }

        /// <summary>
        /// ✅ Yeni kullanıcı oluştur
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] UserDto userDto)
        {
            if (await _context.Users.AnyAsync(u => u.Username == userDto.Username))
                return BadRequest(new { message = "Bu kullanıcı adı zaten kullanılıyor." });

            var user = new User
            {
                Username = userDto.Username,
                Password = HashPassword(userDto.Password),
                Role = userDto.Role ?? "User",
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }

        /// <summary>
        /// ✅ Kullanıcıyı güncelle
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserDto updatedUser)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { message = "Güncellenecek kullanıcı bulunamadı." });

            user.Username = updatedUser.Username ?? user.Username;
            user.Role = updatedUser.Role ?? user.Role;
            user.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(updatedUser.Password))
                user.Password = HashPassword(updatedUser.Password);

            await _context.SaveChangesAsync();
            return Ok(new { message = "Kullanıcı başarıyla güncellendi." });
        }

        /// <summary>
        /// ✅ Kullanıcıyı sil
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { message = "Silinecek kullanıcı bulunamadı." });

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Kullanıcı başarıyla silindi." });
        }

        /// <summary>
        /// 🔑 Şifre Hashleme Fonksiyonu (SHA-256)
        /// </summary>
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        /// <summary>
        /// 🔑 JWT Token Oluşturma
        /// </summary>
        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["JwtSettings:Secret"]); // ✅ appsettings.json'dan çekiliyor

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddMinutes(Convert.ToInt32(_configuration["JwtSettings:AccessTokenExpirationMinutes"])), // ✅ Süre dinamik oldu
                Issuer = _configuration["JwtSettings:Issuer"],
                Audience = _configuration["JwtSettings:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}

// ✅ Kullanıcı DTO Modeli
public class UserDto
{
    public string Username { get; set; }
    public string Password { get; set; }
    public string? Role { get; set; }
}

// ✅ Giriş İsteği Modeli
public class LoginRequest
{
    public string Username { get; set; }
    public string Password { get; set; }
}
