using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SampleWebApiAspNetCore.Authentication;
using SampleWebApiAspNetCore.Repositories;
using SampleWebApiAspNetCore.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace SampleWebApiAspNetCore.Controllers.v1
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticateController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IConfiguration _configuration;
        private readonly MyDbContext _context;
        private readonly HasherService _hasher;
        public AuthenticateController(MyDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration, HasherService hasher)
        {
            _context = context;
            this.userManager = userManager;
            this.roleManager = roleManager;
            _configuration = configuration;
            _hasher = hasher;
        }

        [HttpPost]
        [Route("login")]
        public IActionResult Login([FromBody] LoginModel model)
        {
            /* var user = await userManager.FindByNameAsync(model.UserName);
             if (user != null && await userManager.CheckPasswordAsync(user, model.Password))*/
            
            var user = _context.Users.FirstOrDefault(u => u.UserName == model.UserName);
   
            if (user != null)
            {
                var verifyPassword = _hasher.VerifyPassword(model.Password, user.Password);
                if(verifyPassword == true)
                {
                    /* string[] userRoles = {"admin", "user"};*/

                    var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

                    /* foreach (var userRole in userRoles)
                     {
                         authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                     }*/

                    var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:SecretKey"]));

                    var token = new JwtSecurityToken(
                        issuer: _configuration["JWT:ValidIssuer"],
                        audience: _configuration["JWT:ValidAudience"],
                        expires: DateTime.Now.AddHours(3),
                        claims: authClaims,
                        signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                        );

                    return Ok(new
                    {
                        token = new JwtSecurityTokenHandler().WriteToken(token),
                        expiration = token.ValidTo
                    });
                }
                else
                {
                    return Unauthorized();
                }
            }
            return NotFound();
        }

        [HttpPost]
        [Route("change-password")]
        public IActionResult ChangePass([FromBody] LoginModel model)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserName == model.UserName);
            if (user != null)
            {
                var encryptedPassword = _hasher.HashPassword(model.Password);
                user.Password = encryptedPassword;
                _context.SaveChanges();
                return Ok("You changed password successful!!");
            }
            else
            {
                return BadRequest();
            }
        }
    }
}
