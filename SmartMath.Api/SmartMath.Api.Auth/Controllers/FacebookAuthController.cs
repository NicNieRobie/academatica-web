using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SmartMath.Api.Auth.Configuration;
using SmartMath.Api.Auth.DTOs;
using SmartMath.Api.Auth.Services;
using SmartMath.Api.Auth.Services.Interfaces;
using SmartMath.Api.Common.Data;
using SmartMath.Api.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SmartMath.Api.Auth.Controllers
{
    [Route("api/auth/facebook")]
    [ApiController]
    public class FacebookAuthController : Controller
    {
        private readonly IOptions<FacebookAuthConfig> _facebookAuthConfig;
        private readonly UserManager<User> _userManager;
        private readonly SmartMathDbContext _authDbContext;
        private readonly ITokenService _tokenService;
        private static readonly HttpClient _httpClient = new HttpClient();

        public FacebookAuthController(
            IOptions<FacebookAuthConfig> facebookAuthConfig, 
            UserManager<User> userManager,
            SmartMathDbContext authDbContext,
            ITokenService tokenService)
        {
            _facebookAuthConfig = facebookAuthConfig;
            _userManager = userManager;
            _authDbContext = authDbContext;
            _tokenService = tokenService;
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] FacebookRegistrationRequestDto registrationRequestDto)
        {
            if (ModelState.IsValid)
            {
                var appAccessTokenResponse = await _httpClient.GetStringAsync($"https://graph.facebook.com/oauth/access_token?client_id={_facebookAuthConfig.Value.AppId}&client_secret={_facebookAuthConfig.Value.Secret}&grant_type=client_credentials");
                var appAccessToken = JsonConvert.DeserializeObject<FacebookApiAppAccessTokenResponseDto>(appAccessTokenResponse);
                var userAccessTokenValidationResponse = await _httpClient.GetStringAsync($"https://graph.facebook.com/debug_token?input_token={registrationRequestDto.AccessToken}&access_token={appAccessToken.AccessToken}");
                var userAccessTokenValidation = JsonConvert.DeserializeObject<FacebookUserAccessTokenValidationResponseDto>(userAccessTokenValidationResponse);

                if (!userAccessTokenValidation.Data.IsValid)
                {
                    return BadRequest(new FacebookRegistrationResponseDto
                    {
                        Success = false,
                        Errors = new List<string>()
                        {
                            "Invalid Facebook user access token"
                        }
                    });
                }

                var userDataResponse = await _httpClient.GetStringAsync($"https://graph.facebook.com/v2.8/me?fields=id,email,first_name,last_name,name,gender,locale,birthday,picture&access_token={registrationRequestDto.AccessToken}");
                var userData = JsonConvert.DeserializeObject<FacebookUserDataResponseDto>(userDataResponse);

                var foundUser = await _userManager.FindByLoginAsync("facebook", userData.Id.ToString());

                if (foundUser != null)
                {
                    return BadRequest(new FacebookRegistrationResponseDto
                    {
                        Success = false,
                        Errors = new List<string>()
                        {
                            "An associated account already exists"
                        },
                        AssociatedID = foundUser.Id
                    });
                }

                foundUser = await _userManager.FindByEmailAsync(userData.Email);

                if (foundUser != null)
                {
                    return BadRequest(new FacebookRegistrationResponseDto
                    {
                        Success = false,
                        Errors = new List<string>()
                        {
                            "Email address is already taken"
                        }
                    });
                }

                var newUser = new User
                {
                    Email = userData.Email,
                    UserName = registrationRequestDto.Username,
                    FirstName = userData.FirstName,
                    LastName = userData.LastName,
                    RegisteredAt = DateTime.Now
                };
                var userCreated = await _userManager.CreateAsync(newUser, null);
                await _userManager.AddLoginAsync(newUser, new UserLoginInfo("facebook", userData.Id.ToString(), "Facebook"));

                if (userCreated.Succeeded)
                {
                    var response = await _tokenService.AuthWithToken(newUser);

                    return Ok(new FacebookRegistrationResponseDto()
                    {
                        Success = true,
                        Token = response.Token,
                        RefreshToken = response.RefreshToken
                    });
                }

                return new JsonResult(new FacebookRegistrationResponseDto()
                {
                    Success = false,
                    Errors = userCreated.Errors.Select(x => x.Description).ToList()
                })
                { StatusCode = 500 };
            }

            return BadRequest(new FacebookRegistrationResponseDto()
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
