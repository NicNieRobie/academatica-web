using Academatica.Api.Common.Data;
using Academatica.Api.Common.Models;
using Academatica.Api.Users.Configuration;
using Academatica.Api.Users.DTOs;
using Academatica.Api.Users.Extensions;
using Academatica.Api.Users.Services;
using AspNetCore.Yandex.ObjectStorage;
using AspNetCore.Yandex.ObjectStorage.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Academatica.Api.Users.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("api/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly YandexStorageService _yandexStorageService;
        private readonly AcadematicaDbContext _academaticaDbContext;
        private readonly UserManager<User> _userManager;
        private readonly IConfirmationCodeManager _confirmationCodeManager;
        private readonly IUserEmailService _userEmailService;

        public UsersController(
            YandexStorageService yandexStorageService,
            AcadematicaDbContext academaticaDbContext,
            UserManager<User> userManager,
            IConfirmationCodeManager confirmationCodeManager,
            IUserEmailService userEmailService)
        {
            _yandexStorageService = yandexStorageService;
            _academaticaDbContext = academaticaDbContext;
            _userManager = userManager;
            _confirmationCodeManager = confirmationCodeManager;
            _userEmailService = userEmailService;
        }

        [HttpPatch]
        [Route("{id}/image")]
        public async Task<IActionResult> SetUserProfileImage(Guid id, [FromForm] IFormFile formFile)
        {
            var userId = User.FindFirst("sub")?.Value;

            if (userId != id.ToString())
            {
                return Forbid("Access denied - token subject invalid.");
            }

            var user = _academaticaDbContext.Users.Where(x => x.Id == id).First();

            if (user == null)
            {
                return BadRequest("Invalid user ID.");
            }

            long length = formFile.Length;
            if (length < 0)
            {
                return BadRequest("Invalid file format.");
            }

            using var fileStream = formFile.OpenReadStream();
            byte[] bytes = new byte[length];
            fileStream.Read(bytes, 0, (int)formFile.Length);

            S3PutResponse response = await _yandexStorageService.PutObjectAsync(bytes, $"Users/{id}/pic.jpeg");

            if (response.IsSuccessStatusCode)
            {
                user.ProfilePicUrl = response.Result;
                await _academaticaDbContext.SaveChangesAsync();
                return Ok(response.Result);
            } else
            {
                return BadRequest("Could not upload the file.");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var user = _academaticaDbContext.Users.Where(x => x.Id == id).First();

            if (user == null)
            {
                return BadRequest("Invalid user ID.");
            }

            _academaticaDbContext.Users.Remove(user);
            await _academaticaDbContext.SaveChangesAsync();

            return Ok($"User {id} deleted");
        }

        [HttpPatch]
        [Route("{id}/username")]
        public async Task<IActionResult> SetUserUsername(Guid id, [FromBody] UserChangeUsernameRequestDto changeUsernameRequestDTO)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirst("sub")?.Value;

                if (userId != id.ToString())
                {
                    return Forbid("Access denied - token subject invalid.");
                }

                var user = _academaticaDbContext.Users.Where(x => x.Id == id).First();

                if (user == null)
                {
                    return BadRequest("Invalid user ID.");
                }

                var username = changeUsernameRequestDTO.Username;
                if (_academaticaDbContext.Users.Where(x => x.UserName == username).Any())
                {
                    return BadRequest("Username is already taken.");
                } else
                {
                    await _userManager.SetUserNameAsync(user, username);

                    return Ok();
                }
            } else
            {
                var message = string.Join(" | ", ModelState.Values
                                    .SelectMany(v => v.Errors)
                                    .Select(e => e.ErrorMessage));
                return BadRequest(message);
            }
        }

        [HttpPatch]
        [Route("{id}/email")]
        public async Task<IActionResult> SetUserEmail(Guid id, [FromBody] UserChangeEmailRequestDto changeEmailRequestDTO)
        {
            if (ModelState.IsValid) 
            {
                var userId = User.FindFirst("sub")?.Value;

                if (userId != id.ToString())
                {
                    return Forbid("Access denied - token subject invalid.");
                }

                var user = _academaticaDbContext.Users.Where(x => x.Id == id).First();

                if (user == null)
                {
                    return BadRequest("Invalid user ID.");
                }

                if (string.IsNullOrEmpty(changeEmailRequestDTO.EMail))
                {
                    return BadRequest("E-Mail not specified.");
                }

                var registeredUser = await _userManager.FindByEmailAsync(changeEmailRequestDTO.EMail);
                if (registeredUser != null)
                {
                    return BadRequest("E-Mail already taken.");
                }

                if (string.IsNullOrEmpty(changeEmailRequestDTO.ConfirmationCode))
                {
                    return Ok(new UserChangeEmailResponseDto 
                    {
                        ConfirmationPending = true,
                        Success = true
                    });
                } else 
                {
                    var cachedCode = await _confirmationCodeManager.GetConfirmationCode(user.Id);
                    if (string.IsNullOrEmpty(cachedCode)) 
                    {
                        return Ok(new UserChangeEmailResponseDto 
                        {
                            ConfirmationPending = true,
                            Success = false
                        });
                    } else {
                        if (cachedCode != changeEmailRequestDTO.ConfirmationCode) 
                        {
                            return Ok(new UserChangeEmailResponseDto 
                            {
                                ConfirmationPending = false,
                                Success = false
                            });
                        }

                        var oldEmail = user.Email;

                        await _userManager.SetEmailAsync(user, changeEmailRequestDTO.EMail);

                        var rollbackCode = await _userManager.GenerateChangeEmailTokenAsync(user, oldEmail);
                        var rollbackUrl = Url.Action(nameof(RollbackUserEmailChange), "Users", new { id = user.Id, code = rollbackCode, oldEmail = oldEmail },
                            protocol: HttpContext.Request.Scheme);
                        await _userEmailService.SendEmailChangeNotification(user, oldEmail, changeEmailRequestDTO.EMail, rollbackUrl);

                        var confirmationCode = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        var callbackUrl = Url.Action(nameof(ConfirmUserEmailChange), "Users", new { id = user.Id, code = confirmationCode }, protocol: HttpContext.Request.Scheme);
                        await _userEmailService.SendNewEmailConfirmation(user, changeEmailRequestDTO.EMail, callbackUrl);

                        await _confirmationCodeManager.RemoveConfirmationCode(user.Id);

                        return Ok(new UserChangeEmailResponseDto 
                        {
                            ConfirmationPending = false,
                            Success = true
                        });
                    }
                }
            } else 
            {
                var message = string.Join(" | ", ModelState.Values
                                    .SelectMany(v => v.Errors)
                                    .Select(e => e.ErrorMessage));
                return BadRequest(message);
            }
        }

        [HttpPost]
        [Route("{id}/email/confirmation-code")]
        public async Task<IActionResult> SendUserEmailConfirmationCode(Guid id)
        {
            var userId = User.FindFirst("sub")?.Value;

            if (userId != id.ToString())
            {
                return Forbid("Access denied - token subject invalid.");
            }

            var user = _academaticaDbContext.Users.Where(x => x.Id == id).First();

            if (user == null)
            {
                return BadRequest("Invalid user ID.");
            }

            var code = await _confirmationCodeManager.CreateConfirmationCode(user.Id);
            await _userEmailService.SendConfirmationCode(user, code);

            return Ok();
        }

        [HttpGet]
        [Route("{id}/email/confirmation-code")]
        public async Task<IActionResult> CheckUserEmailConfirmationCode(Guid id, [FromQuery] string code)
        {
            var userId = User.FindFirst("sub")?.Value;

            if (userId != id.ToString())
            {
                return Forbid("Access denied - token subject invalid.");
            }

            var user = _academaticaDbContext.Users.Where(x => x.Id == id).First();

            if (user == null)
            {
                return BadRequest("Invalid user ID.");
            }

            if (code == null)
            {
                return BadRequest("No confirmation code specified.");
            }

            var cachedCode = await _confirmationCodeManager.GetConfirmationCode(user.Id);

            if (cachedCode == code)
            {
                return Ok(new CheckUserEmailConfirmationCodeResponseDto
                {
                    Success = true
                });
            } else
            {
                return Ok(new CheckUserEmailConfirmationCodeResponseDto
                {
                    Success = false
                });
            }
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("{id}/email/confirm")]
        public async Task<IActionResult> ConfirmUserEmailChange(Guid id, string code)
        {
            if (code == null)
            {
                return Redirect("https://localhost:5011/email-not-confirmed");
            }
            var user = await _userManager.FindByIdAsync(id.ToString());
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

        [AllowAnonymous]
        [HttpGet]
        [Route("{id}/email/rollback")]
        public async Task<IActionResult> RollbackUserEmailChange(Guid id, string code, string oldEmail)
        {
            if (code == null)
            {
                return Redirect("https://localhost:5011/error");
            }
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return Redirect("https://localhost:5011/error");
            }

            Console.WriteLine(" -> CODE ON CHECK: " + code);

            var result = await _userManager.ChangeEmailAsync(user, oldEmail, code);

            if (result.Succeeded)
            {
                return Redirect("https://localhost:5011/email-reverted");
            }

            return Redirect("https://localhost:5011/error");
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetUserProfile(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return BadRequest("Invalid user ID.");
            }

            var userStats = _academaticaDbContext.UserStats.Where(x => x.UserId == id).First();
            if (userStats == null)
            {
                return BadRequest("Stats entry could not be found.");
            }

            return Ok(new GetUserProfileResponseDto()
            {
                Username = user.UserName,
                ProfilePicUrl = user.ProfilePicUrl,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Exp = userStats.UserExp,
                ExpThisWeek = userStats.UserExpThisWeek
            });
        }
    }
}
