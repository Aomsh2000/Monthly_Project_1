using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HealthSystem.Data;
using HealthSystem.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HealthSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        //inject AppDbContext and IConfiguration
        private readonly AppDbContext _context;
        private readonly string _jwtSecret;
        private readonly string _jwtIssuer;
        private readonly string _jwtAudience;
        private readonly IMemoryCache _cache;

        public AuthController(AppDbContext context, IConfiguration configuration, IMemoryCache cache)
        {
            _context = context;
            _jwtSecret = configuration["Jwt:SecretKey"];
            _jwtIssuer = configuration["Jwt:Issuer"];
            _jwtAudience = configuration["Jwt:Audience"];
            _cache = cache;
        }

        // Sign In Endpoint
        [HttpPost("signin")]
        public async Task<IActionResult> SignIn([FromBody] SignIn signInRequest)
        {
            var cacheKey = $"user_{signInRequest.Email}";
            if (!_cache.TryGetValue(cacheKey, out User user))
            {
                user = await _context.Users.FirstOrDefaultAsync(u => u.Email == signInRequest.Email);
                if (user != null)
                {
                    var cacheOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromMinutes(30));
                    _cache.Set(cacheKey, user, cacheOptions);
                }
            }

            if (user == null || !BCrypt.Net.BCrypt.Verify(signInRequest.Password, user.Password))
            {
                return Unauthorized("Invalid Email or password.");
            }
            //Generate the token
            var token = GenerateJwtToken(user);

            return Ok(new { Token = token, Role = user.Role.ToString(), ID = user.UserID });
        }
        
        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                //Define the claims
                new Claim(JwtRegisteredClaimNames.Sub, user.UserID.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FirstName),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtIssuer,
                audience: _jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}