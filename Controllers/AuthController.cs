using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ApiEmprendimiento.Context;
using ApiEmprendimiento.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiEmprendimiento.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == loginDto.Email);

            if (usuario == null || !BCrypt.Net.BCrypt.Verify(loginDto.Contrasena, usuario.Contrasena))
                return Unauthorized("Credenciales inválidas");

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim("EmprendimientoId", usuario.EmprendimientoId.ToString())
            };

            var creds = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Issuer"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return Ok(new
            {
                token = tokenHandler.WriteToken(token),
                expiration = token.ValidTo
            });
        }
    }

    public class LoginDto
    {
        public string Email { get; set; }
        public string Contrasena { get; set; }
    }
}
