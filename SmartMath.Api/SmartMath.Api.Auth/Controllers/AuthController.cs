using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartMath.Api.Auth.Configuration;
using SmartMath.Api.Auth.DTOs;
using SmartMath.Api.Auth.Models;
using SmartMath.Api.Auth.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartMath.Api.Auth.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly ITokenService _tokenService;

        public AuthController(UserManager<User> userManager, ITokenService tokenService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegistrationRequestDto registrationRequestDto)
        {
            if (ModelState.IsValid)
            {
                var registeredUser = await _userManager.FindByEmailAsync(registrationRequestDto.Email);

                if (registeredUser != null)
                {
                    return BadRequest(new RegistrationResponseDto() 
                    { 
                        Success = false, Errors = new List<string> 
                        {
                            "Email address is already taken"
                        } 
                    });
                }

                var newUser = new User
                {
                    Email = registrationRequestDto.Email,
                    UserName = registrationRequestDto.Username,
                    FirstName = registrationRequestDto.FirstName,
                    LastName = registrationRequestDto.LastName,
                    RegisteredAt = DateTime.Now
                };
                var userCreated = await _userManager.CreateAsync(newUser, registrationRequestDto.Password);

                if (userCreated.Succeeded)
                {
                    var accessToken = _tokenService.GenerateAccessToken(newUser);

                    return Ok(new RegistrationResponseDto()
                    {
                        Success = true,
                        Token = accessToken,
                        RefreshToken = ""
                    });
                }

                return new JsonResult(new RegistrationResponseDto()
                {
                    Success = false,
                    Errors = userCreated.Errors.Select(x => x.Description).ToList()
                }) { StatusCode = 500 };
            }

            return BadRequest(new RegistrationResponseDto()
            {
                Success = false,
                Errors = new List<string>()
                { 
                    "Invalid request payload"
                }
            });
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] AuthRequestDto authRequestDto)
        {
            if (ModelState.IsValid)
            {
                var registeredUser = await _userManager.FindByEmailAsync(authRequestDto.Email);

                if (registeredUser == null)
                {
                    return BadRequest(new AuthResponseDto
                    {
                        Success = false,
                        Errors = new List<string>
                        {
                            "User not found"
                        }
                    });
                }

                var passwordIsCorrect = await _userManager.CheckPasswordAsync(registeredUser, authRequestDto.Password);

                if (passwordIsCorrect)
                {
                    var jwtToken = _tokenService.GenerateAccessToken(registeredUser);

                    return Ok(new AuthResponseDto
                    {
                        Success = true,
                        Token = jwtToken
                    });
                } else
                {
                    return BadRequest(new AuthResponseDto
                    {
                        Success = false,
                        Errors = new List<string>
                        {
                            "Invalid password"
                        }
                    });
                }
            }

            return BadRequest(new AuthResponseDto
            {
                Success = false,
                Errors = new List<string>
                {
                    "Invalid request payload"
                }
            });
        }

        [HttpGet("verify")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> VerifyToken()
        {
            var userID = User.Claims.FirstOrDefault();

            if (userID == null)
            {
                return Unauthorized();
            }

            var userExists = await _userManager.FindByIdAsync(userID.Value);

            if (userExists == null)
            {
                return Unauthorized();
            }

            return NoContent();
        }
    }
}
