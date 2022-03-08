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
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AspNetCore.Yandex.ObjectStorage;
using AspNetCore.Yandex.ObjectStorage.Models;
using System.Security.Claims;
using IdentityModel;
using Academatica.Api.Auth.AuthManagement;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Configuration;

namespace Academatica.Api.Auth.Controllers
{
    /// <summary>
    /// Controller responsible for handling authentication requests for API connection.
    /// </summary>
    [Route("connect")]
    public class ConnectController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly AcadematicaDbContext _academaticaDbContext;
        private readonly IEmailSender _emailSender;
        private readonly IWebHostEnvironment _env;
        private readonly RoleManager<AcadematicaRole> _roleManager;
        private readonly YandexStorageService _yandexStorageService;
        private readonly IConfiguration _configuration;

        public ConnectController(
            UserManager<User> userManager,
            AcadematicaDbContext academaticaDbContext,
            RoleManager<AcadematicaRole> roleManager,
            YandexStorageService yandexStorageService,
            IEmailSender emailSender,
            IWebHostEnvironment env,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _academaticaDbContext = academaticaDbContext;
            _emailSender = emailSender;
            _env = env;
            _roleManager = roleManager;
            _yandexStorageService = yandexStorageService;
            _configuration = configuration;
        }

        /// <summary>
        /// User registration endpoint.
        /// </summary>
        /// <param name="registrationRequestDto">User registartion data.</param>
        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromForm] RegistrationRequestDto registrationRequestDto)
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

                    if (registrationRequestDto.ProfilePicture != null)
                    {
                        long length = registrationRequestDto.ProfilePicture.Length;
                        if (length < 0)
                        {
                            return BadRequest("Invalid file format.");
                        }

                        using var fileStream = registrationRequestDto.ProfilePicture.OpenReadStream();
                        byte[] bytes = new byte[length];
                        fileStream.Read(bytes, 0, (int)registrationRequestDto.ProfilePicture.Length);

                        S3PutResponse response = await _yandexStorageService.PutObjectAsync(bytes, $"Users/{newUser.Id}/pic.jpeg");

                        if (response.IsSuccessStatusCode)
                        {
                            newUser.ProfilePicUrl = response.Result;
                            await _academaticaDbContext.SaveChangesAsync();
                        }
                    }

                    return Ok();
                }

                return BadRequest(string.Join(", ", userCreated.Errors.Select(x => x.Description).ToList()));
            }

            return BadRequest("Invalid request payload");
        }

        /// <summary>
        /// User email post-registration confirmation endpoint.
        /// </summary>
        /// <param name="userId">User ID.</param>
        /// <param name="code">Confirmation code (generated automatically).</param>
        [HttpGet]
        [Route("confirm-mail")]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            var website = _configuration["Website"];

            if (userId == null || code == null)
            {
                return Redirect(website + "/email-not-confirmed");
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Redirect(website + "/email-not-confirmed");
            }
            var result = await _userManager.ConfirmEmailAsync(user, code);

            if (result.Succeeded)
            {
                return Redirect(website + "/email-confirmed");
            }

            return Redirect(website + "/email-not-confirmed");
        }
    }
}
