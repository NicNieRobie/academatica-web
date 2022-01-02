using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartMath.Api.Auth.Configuration;
using SmartMath.Api.Auth.DTOs;
using SmartMath.Api.Auth.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartMath.Api.Auth.Controllers
{
    [Route("api/[Controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly JwtConfig _jwtConfiguration;
        private readonly UserManager<User> _userManager;

        public AuthController(JwtConfig jwtConfiguration, UserManager<User> userManager)
        {
            _jwtConfiguration = jwtConfiguration;
            _userManager = userManager;
        }

        [HttpPost]
        [Route("register")]
        public IActionResult Register([FromBody] RegistrationRequestDto registrationRequestDto)
        {
            if (ModelState.IsValid)
            {

            }
        }
    }
}
