using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WhatSharp.Srv.Services;
using WhatSharp.Shared.Models;
using WhatSharp.Srv.Services;

namespace WhatSharp.Srv.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserServiceSrv _userServiceSrv;

        public UsersController(UserServiceSrv userServiceSrv)
        {
            _userServiceSrv = userServiceSrv;
        }

        [HttpPost("register")]
        [Obsolete("Use POST /api/auth/register with RegisterDTO.")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            var sucess = await _userServiceSrv.RegisterUserAsync(user.Login, user.PasswordHash);
            if (!sucess)
                return BadRequest("Login déjà utilisé");

            return Ok();
        }

        [HttpPost("login")]
        [Obsolete("Use POST /api/auth/register with LoginDTO.")]
        public async Task<IActionResult> Login([FromBody] User user)
        {
            var existingUser = await _userServiceSrv.LoginUserAsync(user.Login, user.PasswordHash);
            if (existingUser == null)
                return Unauthorized("Login ou mot de passe incorrect");

            return Ok(existingUser);
        }
    }
}
