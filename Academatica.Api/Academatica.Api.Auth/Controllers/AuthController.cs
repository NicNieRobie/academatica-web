using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using SmartMath.Api.Auth.DTOs;
using SmartMath.Api.Auth.Services;
using SmartMath.Api.Auth.Services.Interfaces;
using SmartMath.Api.Common.Data;
using SmartMath.Api.Common.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SmartMath.Api.Auth.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly ITokenService _tokenService;
        private readonly SmartMathDbContext _smartMathDbContext;
        private readonly IEmailSender _emailSender;
        private IWebHostEnvironment _env;

        public AuthController(
            UserManager<User> userManager,
            ITokenService tokenService,
            SmartMathDbContext smartMathDbContext, 
            IWebHostEnvironment env,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _smartMathDbContext = smartMathDbContext;
            _emailSender = emailSender;
            _env = env;
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
                    _smartMathDbContext.UserStats.Add(new StatsEntry
                    {
                        UserId = newUser.Id,
                        DaysStreak = 0,
                        BuoysLeft = 5,
                        LastClassFinishedAt = null,
                        User = newUser,
                        UserExp = 0
                    });
                    await _smartMathDbContext.SaveChangesAsync();

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);
                    var callbackUrl = Url.Action(nameof(ConfirmEmail), "Auth", new { userId = newUser.Id, code = code }, protocol: HttpContext.Request.Scheme);

                    var webrootPath = _env.WebRootPath;
                    var pathToFile = _env.WebRootPath
                            + Path.DirectorySeparatorChar.ToString()
                            + "resources"
                            + Path.DirectorySeparatorChar.ToString()
                            + "templates"
                            + Path.DirectorySeparatorChar.ToString()
                            + "EmailTemplates"
                            + Path.DirectorySeparatorChar.ToString()
                            + "EMailConfirmTemplate.html";
                    var builder = new BodyBuilder();
                    using (StreamReader SourceReader = System.IO.File.OpenText(pathToFile))
                    {
                        builder.HtmlBody = SourceReader.ReadToEnd();
                    }
                    string messageBody = string.Format(builder.HtmlBody, newUser.UserName, callbackUrl);

                    await _emailSender.SendEmailAsync(newUser.Email, "Подтверждение адреса от учётной записи Academatica", messageBody);

                    var response = await _tokenService.AuthWithToken(newUser);

                    return Ok(new RegistrationResponseDto()
                    {
                        Success = true
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

        [HttpGet]
        [Route("confirm-mail")]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("EmailNotConfirmed");
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return View("EmailNotConfirmed");
            }
            var result = await _userManager.ConfirmEmailAsync(user, code);

            if (result.Succeeded)
            {
                return View("EmailConfirmed");
            }

            return View("EmailNotConfirmed");
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
                    var emailConfirmed = await _userManager.IsEmailConfirmedAsync(registeredUser);

                    if (!emailConfirmed)
                    {
                        return BadRequest(new AuthResponseDto
                        {
                            Success = false,
                            Errors = new List<string>
                            {
                                "Email not confirmed"
                            }
                        });
                    }

                    var response = await _tokenService.AuthWithToken(registeredUser);

                    return Ok(response);
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

        [HttpPost]
        [Route("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequestDto refreshRequestDto)
        {
            if (ModelState.IsValid)
            {
                var validationResults = await _tokenService.IsValidForRefresh(refreshRequestDto);

                if (!validationResults.IsValid)
                {
                    return BadRequest(new AuthResponseDto
                    {
                        Success = false,
                        Errors = new List<string>()
                        {
                            validationResults.Error
                        }
                    });
                }

                var user = await _userManager.FindByIdAsync(validationResults.InvalidatedToken.UserId.ToString());

                var response = await _tokenService.AuthWithToken(user);

                return Ok(response);
            }

            return BadRequest(new AuthResponseDto
            {
                Success = false,
                Errors = new List<string>()
                {
                    "Invalid request payload"
                }
            });
        }
    }
}
