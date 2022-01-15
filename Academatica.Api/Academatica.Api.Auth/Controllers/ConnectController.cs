using Academatica.Api.Auth.DTOs;
using Academatica.Api.Common.Data;
using Academatica.Api.Common.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Academatica.Api.Auth.Controllers
{
    [Route("connect")]
    public class ConnectController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly AcadematicaDbContext _academaticaDbContext;
        private readonly IEmailSender _emailSender;
        private readonly IWebHostEnvironment _env;

        public ConnectController(
            UserManager<User> userManager,
            AcadematicaDbContext academaticDbContext,
            IEmailSender emailSender,
            IWebHostEnvironment env)
        {
            _userManager = userManager;
            _academaticaDbContext = academaticDbContext;
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
                        Success = false,
                        Errors = new List<string>
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
                    _academaticaDbContext.UserStats.Add(new StatsEntry
                    {
                        UserId = newUser.Id,
                        DaysStreak = 0,
                        BuoysLeft = 5,
                        LastClassFinishedAt = null,
                        User = newUser,
                        UserExp = 0
                    });
                    await _academaticaDbContext.SaveChangesAsync();

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);
                    var callbackUrl = Url.Action(nameof(ConfirmEmail), "Connect", new { userId = newUser.Id, code = code }, protocol: HttpContext.Request.Scheme);

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

                    return Ok(new RegistrationResponseDto()
                    {
                        Success = true
                    });
                }

                return new JsonResult(new RegistrationResponseDto()
                {
                    Success = false,
                    Errors = userCreated.Errors.Select(x => x.Description).ToList()
                })
                { StatusCode = 500 };
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
    }
}
