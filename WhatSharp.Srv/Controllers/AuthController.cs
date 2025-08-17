using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WhatSharp.Shared.DTO;
using WhatSharp.Shared.Models;
using WhatSharp.Srv.Data;
using WhatSharp.Srv.Security;

namespace WhatSharp.Srv.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        public AuthController(AppDbContext db) => _db = db;
        
        [HttpPost("register")]
        public async Task<ActionResult<LoginResponseDTO>> Register([FromBody] RegisterDTO dto)
        {
            var login = dto.Login?.Trim();
            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest("Login et mot de passe requis.");

            if (await _db.Users.AnyAsync(u => u.Login == login))
                return Conflict("Login déjà pris.");

            var user = new User
            {
                Login = login,
                PasswordHash = PasswordHasher.Hash(dto.Password)
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return Ok(new LoginResponseDTO { Id = user.Id, Login = user.Login });
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDTO>> Login([FromBody] LoginDTO dto)
        {
            var login = dto.Login?.Trim() ?? "";
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Login == login);
            if (user is null) return Unauthorized("Identifiants invalides.");

            if (!PasswordHasher.Verify(dto.Password ?? "", user.PasswordHash))
                return Unauthorized("Identifiants invalides.");

            return Ok(new LoginResponseDTO { Id = user.Id, Login = user.Login });
        }
    }
}
