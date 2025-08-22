using MedanoClinicBE.DTOs;
using MedanoClinicBE.Helpers;
using MedanoClinicBE.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MedanoClinicBE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userMgr;
        private readonly JwtSettings _jwt;

        public AuthController(UserManager<ApplicationUser> userMgr, JwtSettings jwt)
        {
            _userMgr = userMgr;
            _jwt = jwt;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            try
            {
                if (dto == null || string.IsNullOrEmpty(dto.Email) || string.IsNullOrEmpty(dto.Password))
                {
                    return BadRequest("Email and password are required");
                }

                var user = new ApplicationUser 
                {   
                    UserName = dto.UserName,
                    FirstName = dto.FirstName, 
                    LastName = dto.LastName,
                    Email = dto.Email,
                    DisplayName = dto.DisplayName,
                    DateOfBirth = dto.DateOfBirth,
                    Gender = dto.Gender,
                    CreatedAt = DateTime.UtcNow
                };
                var res = await _userMgr.CreateAsync(user, dto.Password);
                if (!res.Succeeded) return BadRequest(res.Errors);

                // assign Client role
                await _userMgr.AddToRoleAsync(user, "Client");
                return Ok(new { message = "User registered successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
        {
            var user = await _userMgr.FindByEmailAsync(dto.Email);
            if (user == null || !await _userMgr.CheckPasswordAsync(user, dto.Password))
                return Unauthorized("Invalid credentials");

            var roles = await _userMgr.GetRolesAsync(user);
            var userRole = roles.FirstOrDefault() ?? "Client"; // Default to Client if no role found
            
            var claims = new List<Claim> {
                new(JwtRegisteredClaimNames.Sub, user.Id),
                new(JwtRegisteredClaimNames.Email, user.Email),
                new(ClaimTypes.Role, userRole)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(_jwt.ExpiryMinutes);

            var token = new JwtSecurityToken(
              issuer: _jwt.Issuer,
              audience: _jwt.Audience,
              claims: claims,
              expires: expires,
              signingCredentials: creds
            );

            return new AuthResponseDto
            {
                Email = user.Email,
                Role = userRole,
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiry = expires
            };
        }
    }
}
