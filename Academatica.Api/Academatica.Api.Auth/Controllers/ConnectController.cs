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
using System.Security.Principal;
using AspNetCore.Yandex.ObjectStorage;
using AspNetCore.Yandex.ObjectStorage.Models;
using System.Security.Claims;
using IdentityModel;
using Academatica.Api.Auth.AuthManagement;
using System.Text.Encodings.Web;

namespace Academatica.Api.Auth.Controllers
{
    [Route("connect")]
    public class ConnectController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly AcadematicaDbContext _academaticaDbContext;
        private readonly IEmailSender _emailSender;
        private readonly IWebHostEnvironment _env;
        private readonly RoleManager<AcadematicaRole> _roleManager;

        public ConnectController(
            UserManager<User> userManager,
            AcadematicaDbContext academaticaDbContext,
            RoleManager<AcadematicaRole> roleManager,
            IEmailSender emailSender,
            IWebHostEnvironment env)
        {
            _userManager = userManager;
            _academaticaDbContext = academaticaDbContext;
            _emailSender = emailSender;
            _env = env;
            _roleManager = roleManager;
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
                    return BadRequest("Email address is already taken");
                }

                registeredUser = await _userManager.FindByNameAsync(registrationRequestDto.Username);

                if (registeredUser != null)
                {
                    return BadRequest("Username is already taken");
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
                        User = newUser,
                        UserExp = 0
                    });
                    await _academaticaDbContext.SaveChangesAsync();

                    await _userManager.AddClaimsAsync(newUser, new Claim[]
                    {
                        new Claim(JwtClaimTypes.Name, newUser.FirstName + " " + newUser.LastName),
                        new Claim(JwtClaimTypes.GivenName, newUser.FirstName),
                        new Claim(JwtClaimTypes.FamilyName, newUser.LastName),
                        new Claim(JwtClaimTypes.Email, newUser.Email)
                    });

                    await _roleManager.CreateAsync(new AcadematicaRole { Name = ServerRoles.User });

                    await _userManager.AddToRoleAsync(newUser, ServerRoles.User);

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
                    string messageBody = string.Format(builder.HtmlBody, newUser.UserName, HtmlEncoder.Default.Encode(callbackUrl));

                    await _emailSender.SendEmailAsync(newUser.Email, "Подтверждение адреса от учётной записи Academatica", messageBody);

                    return Ok();
                }

                return BadRequest(string.Join(", ", userCreated.Errors.Select(x => x.Description).ToList()));
            }

            return BadRequest("Invalid request payload");
        }

        [HttpGet]
        [Route("confirm-mail")]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return Redirect("https://localhost:5011/email-not-confirmed");
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Redirect("https://localhost:5011/email-not-confirmed");
            }
            var result = await _userManager.ConfirmEmailAsync(user, code);

            if (result.Succeeded)
            {
                return Redirect("https://localhost:5011/email-confirmed");
            }

            return Redirect("https://localhost:5011/email-not-confirmed");
        }
    }
}
