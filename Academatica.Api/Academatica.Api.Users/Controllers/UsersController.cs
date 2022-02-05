using Academatica.Api.Common.Data;
using Academatica.Api.Common.Models;
using Academatica.Api.Users.Configuration;
using Academatica.Api.Users.DTOs;
using Academatica.Api.Users.Extensions;
using Academatica.Api.Users.Services;
using Academatica.Api.Users.Services.SyncDataServices.Http;
using AspNetCore.Yandex.ObjectStorage;
using AspNetCore.Yandex.ObjectStorage.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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
        private readonly IAuthDataClient _authDataClient;

        public UsersController(
            YandexStorageService yandexStorageService,
            AcadematicaDbContext academaticaDbContext,
            UserManager<User> userManager,
            IConfirmationCodeManager confirmationCodeManager,
            IAuthDataClient authDataClient)
        {
            _yandexStorageService = yandexStorageService;
            _academaticaDbContext = academaticaDbContext;
            _userManager = userManager;
            _confirmationCodeManager = confirmationCodeManager;
            _authDataClient = authDataClient;
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

                if (string.IsNullOrEmpty(changeEmailRequestDTO.ConfirmationCode))
                {
                    await _confirmationCodeManager.CreateConfirmationCode(user.Id);

                    try
                    {
                        await _authDataClient.SendEmailConfirmation(new SendConfirmationEmailRequestDto
                        {
                            Email = user.Email,
                            User = user
                        });
                    } catch (Exception ex)
                    {
                        Console.WriteLine("HTTP POST failed.");
                        Console.WriteLine(ex.Message);
                    }

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
                            Confirmed = false,
                            Success = false
                        });
                    } else {
                        if (cachedCode != changeEmailRequestDTO.ConfirmationCode) 
                        {
                            return Ok(new UserChangeEmailResponseDto 
                            {
                                ConfirmationPending = false,
                                Confirmed = false,
                                Success = false
                            });
                        }

                        await _userManager.SetEmailAsync(user, changeEmailRequestDTO.EMail);

                        return Ok(new UserChangeEmailResponseDto 
                        {
                            ConfirmationPending = false,
                            Confirmed = false,
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
    }
}
