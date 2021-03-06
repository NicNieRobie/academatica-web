using Academatica.Api.Common.Data;
using Academatica.Api.Common.Models;
using Academatica.Api.Users.DTOs;
using Academatica.Api.Users.Services;
using AspNetCore.Yandex.ObjectStorage;
using AspNetCore.Yandex.ObjectStorage.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly IAchievementsManager _achievementsManager;
        private readonly IConfiguration _configuration;

        public UsersController(
            YandexStorageService yandexStorageService,
            AcadematicaDbContext academaticaDbContext,
            UserManager<User> userManager,
            IConfirmationCodeManager confirmationCodeManager,
            IUserEmailService userEmailService,
            IAchievementsManager achievementsManager,
            IConfiguration configuration)
        {
            _yandexStorageService = yandexStorageService;
            _academaticaDbContext = academaticaDbContext;
            _userManager = userManager;
            _confirmationCodeManager = confirmationCodeManager;
            _userEmailService = userEmailService;
            _achievementsManager = achievementsManager;
            _configuration = configuration;
        }

        /// <summary>
        /// Endpoint used to set a user profile image.
        /// </summary>
        /// <param name="id">User ID.</param>
        /// <param name="picture">New user profile picture.</param>
        [HttpPatch]
        [Route("{id}/image")]
        public async Task<IActionResult> SetUserProfileImage(Guid id, [FromForm] IFormFile picture)
        {
            var userId = User.FindFirst("sub")?.Value;

            if (userId != id.ToString())
            {
                return Forbid("Access denied - token subject invalid.");
            }

            var user = _academaticaDbContext.Users.Where(x => x.Id == id).FirstOrDefault();

            if (user == null)
            {
                return BadRequest("Invalid user ID.");
            }

            long length = picture.Length;
            if (length < 0)
            {
                return BadRequest("Invalid file format.");
            }

            using var fileStream = picture.OpenReadStream();
            byte[] bytes = new byte[length];
            fileStream.Read(bytes, 0, (int)picture.Length);

            S3PutResponse response = await _yandexStorageService.PutObjectAsync(bytes, $"Users/{id}/pic.jpeg");

            if (response.IsSuccessStatusCode)
            {
                user.ProfilePicUrl = response.Result;
                await _academaticaDbContext.SaveChangesAsync();
                return Ok();
            } else
            {
                return BadRequest("Could not upload the file.");
            }
        }

        /// <summary>
        /// Endpoint used by admin to delete a user.
        /// </summary>
        /// <param name="id">User ID.</param>
        [Authorize(Roles = "Admin")]
        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var user = _academaticaDbContext.Users.Where(x => x.Id == id).FirstOrDefault();

            if (user == null)
            {
                return BadRequest("Invalid user ID.");
            }

            _academaticaDbContext.Users.Remove(user);
            await _academaticaDbContext.SaveChangesAsync();

            return Ok($"User {id} deleted");
        }

        /// <summary>
        /// Endpoint used to change the user's username.
        /// </summary>
        /// <param name="id">User ID.</param>
        /// <param name="changeUsernameRequestDTO">Body - new username.</param>
        [HttpPatch]
        [Route("{id}/username")]
        public async Task<IActionResult> SetUserUsername(Guid id, [FromBody] UserChangeUsernameRequestDto changeUsernameRequestDTO)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirst("sub")?.Value;

                if (userId != id.ToString())
                {
                    return Forbid();
                }

                var user = _academaticaDbContext.Users.Where(x => x.Id == id).FirstOrDefault();

                if (user == null)
                {
                    return NotFound("Invalid user ID.");
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

        /// <summary>
        /// Endpoint used to change the user's first name.
        /// </summary>
        /// <param name="id">User ID.</param>
        /// <param name="changeFirstNameRequestDTO">Body - new first name.</param>
        [HttpPatch]
        [Route("{id}/firstname")]
        public async Task<IActionResult> SetUserFirstName(Guid id, [FromBody] UserChangeFirstNameRequestDto changeFirstNameRequestDTO)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirst("sub")?.Value;

                if (userId != id.ToString())
                {
                    return Forbid();
                }

                var user = _academaticaDbContext.Users.Where(x => x.Id == id).FirstOrDefault();

                if (user == null)
                {
                    return NotFound("Invalid user ID.");
                }

                var firstName = changeFirstNameRequestDTO.FirstName;

                user.FirstName = firstName;

                await _academaticaDbContext.SaveChangesAsync();

                return Ok();
            }
            else
            {
                var message = string.Join(" | ", ModelState.Values
                                    .SelectMany(v => v.Errors)
                                    .Select(e => e.ErrorMessage));
                return BadRequest(message);
            }
        }

        /// <summary>
        /// Endpoint used to change the user's first name.
        /// </summary>
        /// <param name="id">User ID.</param>
        /// <param name="changeLastNameRequestDTO">Body - new last name.</param>
        [HttpPatch]
        [Route("{id}/lastname")]
        public async Task<IActionResult> SetUserLastName(Guid id, [FromBody] UserChangeLastNameRequestDto changeLastNameRequestDTO)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirst("sub")?.Value;

                if (userId != id.ToString())
                {
                    return Forbid();
                }

                var user = _academaticaDbContext.Users.Where(x => x.Id == id).FirstOrDefault();

                if (user == null)
                {
                    return NotFound("Invalid user ID.");
                }

                var lastName = changeLastNameRequestDTO.LastName;

                user.LastName = lastName;

                await _academaticaDbContext.SaveChangesAsync();

                return Ok();
            }
            else
            {
                var message = string.Join(" | ", ModelState.Values
                                    .SelectMany(v => v.Errors)
                                    .Select(e => e.ErrorMessage));
                return BadRequest(message);
            }
        }

        /// <summary>
        /// Endpoint used to change user email address.
        /// </summary>
        /// <param name="id">User ID.</param>
        /// <param name="changeEmailRequestDTO">New email address and the confirmation code.</param>
        [HttpPatch]
        [Route("{id}/email")]
        public async Task<IActionResult> SetUserEmail(Guid id, [FromBody] UserChangeEmailRequestDto changeEmailRequestDTO)
        {
            if (ModelState.IsValid) 
            {
                var userId = User.FindFirst("sub")?.Value;

                if (userId != id.ToString())
                {
                    return Forbid();
                }

                var user = _academaticaDbContext.Users.Where(x => x.Id == id).FirstOrDefault();

                if (user == null)
                {
                    return NotFound("Invalid user ID.");
                }

                if (string.IsNullOrEmpty(changeEmailRequestDTO.EMail))
                {
                    return Ok(new UserChangeEmailResponseDto()
                    {
                        Success = false,
                        ConfirmationPending = false,
                        Error = "Email was null."
                    });
                }

                var registeredUser = await _userManager.FindByEmailAsync(changeEmailRequestDTO.EMail);
                if (registeredUser != null)
                {
                    return Ok(new UserChangeEmailResponseDto()
                    {
                        Success = false,
                        ConfirmationPending = false,
                        Error = "Email already taken."
                    });
                }

                if (string.IsNullOrEmpty(changeEmailRequestDTO.ConfirmationCode))
                {
                    return Ok(new UserChangeEmailResponseDto()
                    {
                        Success = false,
                        ConfirmationPending = false,
                        Error = "Confirmation code was null."
                    });
                } else 
                {
                    var cachedCode = await _confirmationCodeManager.GetEmailConfirmationCode(user.Id);
                    if (string.IsNullOrEmpty(cachedCode)) 
                    {
                        return StatusCode(500, "Code cache was null.");
                    } else {
                        if (cachedCode != changeEmailRequestDTO.ConfirmationCode) 
                        {
                            return Ok(new UserChangeEmailResponseDto 
                            {
                                ConfirmationPending = true,
                                Success = false,
                                Error = "Invalid confirmation code."
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

                        await _confirmationCodeManager.RemoveEmailConfirmationCode(user.Id);

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

        /// <summary>
        /// Endpoint used to send an email change confirmation code to the user's email address.
        /// </summary>
        /// <param name="id">User ID.</param>
        [HttpPost]
        [Route("{id}/email/confirmation-code")]
        public async Task<IActionResult> SendUserEmailConfirmationCode(Guid id)
        {
            var userId = User.FindFirst("sub")?.Value;

            if (userId != id.ToString())
            {
                return Forbid();
            }

            var user = _academaticaDbContext.Users.Where(x => x.Id == id).FirstOrDefault();

            if (user == null)
            {
                return NotFound("Invalid user ID.");
            }

            var code = await _confirmationCodeManager.CreateEmailConfirmationCode(user.Id);
            await _userEmailService.SendConfirmationCode(user, code, NotificationType.EmailChangeNotification);

            return Ok();
        }

        /// <summary>
        /// Endpoint used to check if specified code is equal to an email change confirmation code sent to the user's email address.
        /// </summary>
        /// <param name="id">User ID.</param>
        /// <param name="code">Code to check.</param>
        [HttpGet]
        [Route("{id}/email/confirmation-code")]
        public async Task<IActionResult> CheckUserEmailConfirmationCode(Guid id, [FromQuery] string code)
        {
            var userId = User.FindFirst("sub")?.Value;

            if (userId != id.ToString())
            {
                return Forbid();
            }

            var user = _academaticaDbContext.Users.Where(x => x.Id == id).FirstOrDefault();

            if (user == null)
            {
                return NotFound("Invalid user ID.");
            }

            if (code == null)
            {
                return BadRequest("No confirmation code specified.");
            }

            var cachedCode = await _confirmationCodeManager.GetEmailConfirmationCode(user.Id);

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

        /// <summary>
        /// Endpoint used to confirm user email address change.
        /// </summary>
        /// <param name="id">User ID.</param>
        /// <param name="code">Email change code (generated automatically).</param>
        [AllowAnonymous]
        [HttpGet]
        [Route("{id}/email/confirm")]
        public async Task<IActionResult> ConfirmUserEmailChange(Guid id, string code)
        {
            var website = _configuration["Website"];

            if (code == null)
            {
                return Redirect(website + "/email-not-confirmed");
            }
            var user = await _userManager.FindByIdAsync(id.ToString());
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

        /// <summary>
        /// Endpoint used to roll the email address change back.
        /// </summary>
        /// <param name="id">User ID.</param>
        /// <param name="code">Email change code (generated automatically).</param>
        /// <param name="oldEmail">Previous email.</param>
        [AllowAnonymous]
        [HttpGet]
        [Route("{id}/email/rollback")]
        public async Task<IActionResult> RollbackUserEmailChange(Guid id, string code, string oldEmail)
        {
            var website = _configuration["Website"];

            if (code == null)
            {
                return Redirect(website + "/error");
            }
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return Redirect(website + "/error");
            }

            var result = await _userManager.ChangeEmailAsync(user, oldEmail, code);

            if (result.Succeeded)
            {
                return Redirect(website + "/email-reverted");
            }

            return Redirect(website + "/error");
        }

        /// <summary>
        /// Endpoint used to get user profile information (username, profile pic URL, first name, last name, 
        /// experience points, exp points this week, level, exp until the next level).
        /// </summary>
        /// <param name="id">User ID.</param>
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetUserProfile(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound("Invalid user ID.");
            }

            var userStats = _academaticaDbContext.UserStats.Where(x => x.UserId == id).FirstOrDefault();
            if (userStats == null)
            {
                return NotFound("Stats entry could not be found.");
            }

            return Ok(new GetUserProfileResponseDto()
            {
                Email = user.Email,
                Username = user.UserName,
                ProfilePicUrl = user.ProfilePicUrl,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Exp = userStats.UserExp,
                ExpThisWeek = userStats.UserExpThisWeek
            });
        }

        /// <summary>
        /// Endpoint used to get current user state (the amount of buoys left and the duration of a day streak).
        /// </summary>
        /// <param name="id">User ID.</param>
        [HttpGet]
        [Route("{id}/state")]
        public IActionResult GetUserState(Guid id)
        {
            var userStats = _academaticaDbContext.UserStats.Where(x => x.UserId == id).FirstOrDefault();
            if (userStats == null)
            {
                return NotFound("Stats entry could not be found.");
            }

            return Ok(new GetUserStatsResponseDto()
            {
                BuoysLeft = userStats.BuoysLeft,
                DaysStreak = userStats.DaysStreak
            });
        }

        /// <summary>
        /// Endpoint used to get the amount of user's lifebuoys.
        /// </summary>
        /// <param name="id">User ID.</param>
        [HttpGet]
        [Route("{id}/buoys")]
        public IActionResult GetUserBuoys(Guid id)
        {
            var userStats = _academaticaDbContext.UserStats.Where(x => x.UserId == id).FirstOrDefault();
            if (userStats == null)
            {
                return NotFound("Stats entry could not be found.");
            }

            return Ok(new GetUserBuoysResponseDto()
            {
                BuoysLeft = userStats.BuoysLeft
            });
        }

        /// <summary>
        /// Endpoint used to decrease the amount of lifebuoys user has.
        /// </summary>
        /// <param name="id">User ID.</param>
        [HttpPatch]
        [Route("{id}/buoys")]
        public async Task<IActionResult> DecreaseUserBuoys(Guid id)
        {
            var userStats = _academaticaDbContext.UserStats.Where(x => x.UserId == id).FirstOrDefault();
            if (userStats == null)
            {
                return NotFound("Stats entry could not be found.");
            }

            userStats.BuoysLeft -= userStats.BuoysLeft == 0 ? 0u : 1u;

            await _academaticaDbContext.SaveChangesAsync();

            return Ok();
        }

        /// <summary>
        /// Endpoint used to get user's ID from user's email if possible.
        /// </summary>
        /// <param name="email">User email.</param>
        [HttpGet]
        [Route("id/{email}")]
        public async Task<IActionResult> FindUserByEmail([FromQuery] string email)
        {
            if (email == null)
            {
                return BadRequest("Email was null.");
            }

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return Ok(new FindUserByEmailResponseDto()
                {
                    Success = false,
                    UserId = null
                });
            }

            return Ok(new FindUserByEmailResponseDto()
            {
                Success = true,
                UserId = user.Id
            });
        }

        /// <summary>
        /// Endpoint used to send a password change confirmation code to user's email address.
        /// </summary>
        /// <param name="id">User ID.</param>
        [HttpPost]
        [Route("{id}/password/confirmation-code")]
        public async Task<IActionResult> SendUserPasswordConfirmationCode(Guid id)
        {
            var userId = User.FindFirst("sub")?.Value;

            if (userId != id.ToString())
            {
                return Forbid();
            }

            var user = _academaticaDbContext.Users.Where(x => x.Id == id).FirstOrDefault();

            if (user == null)
            {
                return NotFound("Invalid user ID.");
            }

            var code = await _confirmationCodeManager.CreatePasswordConfirmationCode(user.Id);
            await _userEmailService.SendConfirmationCode(user, code, NotificationType.PasswordChangeNotification);

            return Ok();
        }

        /// <summary>
        /// Endpoint used to check if specified code is equal to a password change confirmation code sent to user's email address.
        /// </summary>
        /// <param name="id">User ID.</param>
        /// <param name="code">Code to check.</param>
        [HttpGet]
        [Route("{id}/password/confirmation-code")]
        public async Task<IActionResult> CheckUserPasswordConfirmationCode(Guid id, [FromQuery] string code)
        {
            var userId = User.FindFirst("sub")?.Value;

            if (userId != id.ToString())
            {
                return Forbid();
            }

            var user = _academaticaDbContext.Users.Where(x => x.Id == id).First();

            if (user == null)
            {
                return NotFound("Invalid user ID.");
            }

            if (code == null)
            {
                return BadRequest("No confirmation code specified.");
            }

            var cachedCode = await _confirmationCodeManager.GetPasswordConfirmationCode(user.Id);

            if (cachedCode == code)
            {
                return Ok(new CheckUserPasswordConfirmationCodeResponseDto
                {
                    Success = true
                });
            }
            else
            {
                return Ok(new CheckUserPasswordConfirmationCodeResponseDto
                {
                    Success = false
                });
            }
        }

        /// <summary>
        /// Endpoint used to perform pasword reset for specified user with a confirmation code.
        /// </summary>
        /// <param name="id">User ID.</param>
        /// <param name="restorePasswordRequestDto">New password, new password confirmation and confirmation code.</param>
        [HttpPatch]
        [Route("{id}/password/restore")]
        public async Task<IActionResult> RestoreUserPassword(Guid id, [FromBody] UserRestorePasswordRequestDto restorePasswordRequestDto)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirst("sub")?.Value;

                if (userId != id.ToString())
                {
                    return Forbid();
                }

                var user = _academaticaDbContext.Users.Where(x => x.Id == id).FirstOrDefault();

                if (user == null)
                {
                    return NotFound("Invalid user ID.");
                }

                List<string> passwordValidationErrors = new List<string>();

                var validators = _userManager.PasswordValidators;

                foreach (var validator in validators)
                {
                    var result = await validator.ValidateAsync(_userManager, null, restorePasswordRequestDto.NewPassword);

                    if (!result.Succeeded)
                    {
                        foreach (var error in result.Errors)
                        {
                            passwordValidationErrors.Add(error.Description);
                        }
                    }
                }

                if (passwordValidationErrors.Count != 0)
                {
                    return BadRequest(string.Join("; ", passwordValidationErrors));
                }

                if (string.IsNullOrEmpty(restorePasswordRequestDto.ConfirmationCode))
                {
                    return BadRequest("Confirmation code was null.");
                }
                else
                {
                    var cachedCode = await _confirmationCodeManager.GetPasswordConfirmationCode(user.Id);
                    if (string.IsNullOrEmpty(cachedCode))
                    {
                        return StatusCode(500, "Code cache was null.");
                    }
                    else
                    {
                        if (cachedCode != restorePasswordRequestDto.ConfirmationCode)
                        {
                            return Ok(new UserChangeEmailResponseDto
                            {
                                ConfirmationPending = false,
                                Success = false,
                                Error = "Invalid confirmation code."
                            });
                        }

                        var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                        await _userManager.ResetPasswordAsync(user, code, restorePasswordRequestDto.NewPassword);

                        await _userEmailService.SendPasswordChangeNotification(user);

                        await _confirmationCodeManager.RemovePasswordConfirmationCode(user.Id);

                        return Ok(new UserRestorePasswordResponseDto
                        {
                            ConfirmationPending = false,
                            Success = true
                        });
                    }
                }
            }
            else
            {
                var message = string.Join(" | ", ModelState.Values
                                    .SelectMany(v => v.Errors)
                                    .Select(e => e.ErrorMessage));
                return BadRequest(message);
            }
        }

        /// <summary>
        /// Endpoint used to change the user's password given his current password.
        /// </summary>
        /// <param name="id">User ID.</param>
        /// <param name="changePasswordRequestDto">Old password, new password, new password confirmation.</param>
        [HttpPatch]
        [Route("{id}/password/")]
        public async Task<IActionResult> SetUserPassword(Guid id, [FromBody] UserChangePasswordRequestDto changePasswordRequestDto)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirst("sub")?.Value;

                if (userId != id.ToString())
                {
                    return Forbid();
                }

                var user = _academaticaDbContext.Users.Where(x => x.Id == id).FirstOrDefault();

                if (user == null)
                {
                    return NotFound("Invalid user ID.");
                }

                List<string> passwordValidationErrors = new List<string>();

                var validators = _userManager.PasswordValidators;

                foreach (var validator in validators)
                {
                    var result = await validator.ValidateAsync(_userManager, null, changePasswordRequestDto.NewPassword);

                    if (!result.Succeeded)
                    {
                        foreach (var error in result.Errors)
                        {
                            passwordValidationErrors.Add(error.Description);
                        }
                    }
                }

                if (passwordValidationErrors.Count != 0)
                {
                    return BadRequest(string.Join("; ", passwordValidationErrors));
                }

                var oldPasswordValid = await _userManager.CheckPasswordAsync(user, changePasswordRequestDto.OldPassword);

                if (oldPasswordValid)
                {
                    var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                    await _userManager.ResetPasswordAsync(user, code, changePasswordRequestDto.NewPassword);

                    await _userEmailService.SendPasswordChangeNotification(user);

                    return Ok(new UserChangePasswordResponseDto()
                    {
                        Success = true
                    });
                }

                return Ok(new UserChangePasswordResponseDto()
                {
                    Success = false,
                    Error = "Invalid password"
                });
            }
            else
            {
                var message = string.Join(" | ", ModelState.Values
                                    .SelectMany(v => v.Errors)
                                    .Select(e => e.ErrorMessage));
                return BadRequest(message);
            }
        }

        /// <summary>
        /// Endpoint used to get a list of user's achievements.
        /// </summary>
        /// <param name="id">User ID.</param>
        [HttpGet]
        [Route("{id}/achievements")]
        public IActionResult GetUserAchievements(Guid id)
        {
            var user = _academaticaDbContext.Users.Where(x => x.Id == id).FirstOrDefault();

            if (user == null)
            {
                return NotFound("Invalid user ID.");
            }

            return Ok(new GetUserAchievementsResponseDto()
            {
                Achievements = _achievementsManager.GetUserAchievements(id).Select(x => new AchievementDto()
                {
                    Name = x.Name,
                    Description = x.Description,
                    ImageUrl = x.ImageUrl,
                    AchievedAmount = x.AchievedAmount
                })
            });
        }
    }
}
