using AuthorizeTest.dntipsAPI.Context;
using AuthorizeTest.dntipsAPI.Entities;
using AuthorizeTest.dntipsAPI.Models;
using AuthorizeTest.dntipsAPI.Services;
using AuthorizeTest.dntipsAPI.Utils;
using AuthorizeTest.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuthorizeTest.dntipsAPI.Controllers
{
    [Route("[controller]")]
    [Authorize]
    [EnableCors("CorsPolicy")]
    public class AccountController : Controller
    {
        private readonly IUsersService _usersService;
        private readonly ITokenStoreService _tokenStoreService;
        private readonly IUnitOfWork _uow;
        private readonly IAntiForgeryCookieService _antiforgery;
        private readonly ITokenFactoryService _tokenFactoryService;

        public AccountController(
            IUsersService usersService,
            ITokenStoreService tokenStoreService,
            ITokenFactoryService tokenFactoryService,
            IUnitOfWork uow,
            IAntiForgeryCookieService antiforgery)
        {
            _usersService = usersService;
            _usersService.CheckArgumentIsNull(nameof(usersService));

            _tokenStoreService = tokenStoreService;
            _tokenStoreService.CheckArgumentIsNull(nameof(tokenStoreService));

            _uow = uow;
            _uow.CheckArgumentIsNull(nameof(_uow));

            _antiforgery = antiforgery;
            _antiforgery.CheckArgumentIsNull(nameof(antiforgery));

            _tokenFactoryService = tokenFactoryService;
            _tokenFactoryService.CheckArgumentIsNull(nameof(tokenFactoryService));
        }

        [AllowAnonymous]
        [IgnoreAntiforgeryToken]
        [HttpPost("[action]")]
        public async Task<IActionResult> Login([FromBody] User loginUser)
        {
            if (loginUser == null)
            {
                return BadRequest("user is not set.");
            }

            var user = await _usersService.FindUserAsync(loginUser.Username, loginUser.Password);
            if (user?.IsActive != true)
            {
                return Unauthorized();
                //return Ok(new AuthenticationResponseDTO
                //{
                //    JWTToken = null,
                //    IsAuthSuccessful = false,
                //    ErrorMessage = "ورود ناموفق"
                //,
                //    UserDTO =null
                //});
            }

            var result = await _tokenFactoryService.CreateJwtTokensAsync(user);
            await _tokenStoreService.AddUserTokenAsync(user, result.RefreshTokenSerial, result.AccessToken, null);
            await _uow.SaveChangesAsync();

            _antiforgery.RegenerateAntiForgeryCookies(result.Claims);

            return Ok(new AuthenticationResponseDTO {JWTToken=new JWTTokenDTO { 
                AccessToken = result.AccessToken, RefreshToken = result.RefreshToken 
                ,AccessTokenExpirationMinutes=result.AccessTokenExpirationMinutes,RefreshTokenExpirationMinutes=result.RefreshTokenExpirationMinutes},
                IsAuthSuccessful=true,ErrorMessage=""
                ,UserDTO=new UserDTO() {
                    Id=user.Id.ToString(), Name=user.DisplayName,Email="",PhoneNo=""
                }  });
        }

        [AllowAnonymous]
        [IgnoreAntiforgeryToken]
        [HttpPost("[action]")]
        public async Task<IActionResult> RefreshToken([FromBody] JWTTokenDTO model)
        {
            var refreshTokenValue = model.RefreshToken;
            if (string.IsNullOrWhiteSpace(refreshTokenValue))
            {
                return BadRequest("refreshToken is not set.");
            }

            var token = await _tokenStoreService.FindTokenAsync(refreshTokenValue);
            if (token == null)
            {
                return Unauthorized();
            }

            var result = await _tokenFactoryService.CreateJwtTokensAsync(token.User);
            await _tokenStoreService.AddUserTokenAsync(token.User, result.RefreshTokenSerial, result.AccessToken, _tokenFactoryService.GetRefreshTokenSerial(refreshTokenValue));
            await _uow.SaveChangesAsync();

            _antiforgery.RegenerateAntiForgeryCookies(result.Claims);

            return Ok(new AuthenticationResponseDTO
            {
                JWTToken = new JWTTokenDTO
                {
                    AccessToken = result.AccessToken,
                    RefreshToken = result.RefreshToken
                ,
                    AccessTokenExpirationMinutes = result.AccessTokenExpirationMinutes,
                    RefreshTokenExpirationMinutes = result.RefreshTokenExpirationMinutes
                },
                IsAuthSuccessful = true,
                ErrorMessage = ""
               ,
                UserDTO = new UserDTO()
                {
                    Id = "0",
                    Name = "",
                    Email = "",
                    PhoneNo = ""
                }
            });
        }

        [AllowAnonymous]
        [HttpGet("[action]")]
        public async Task<bool> Logout(string refreshToken)
        {
            var claimsIdentity = this.User.Identity as ClaimsIdentity;
            var userIdValue = claimsIdentity.FindFirst(ClaimTypes.UserData)?.Value;

            // The Jwt implementation does not support "revoke OAuth token" (logout) by design.
            // Delete the user's tokens from the database (revoke its bearer token)
            await _tokenStoreService.RevokeUserBearerTokensAsync(userIdValue, refreshToken);
            await _uow.SaveChangesAsync();

            _antiforgery.DeleteAntiForgeryCookies();

            return true;
        }

        [HttpGet("[action]"), HttpPost("[action]")]
        public bool IsAuthenticated()
        {
            return User.Identity.IsAuthenticated;
        }

        [HttpGet("[action]"), HttpPost("[action]")]
        public IActionResult GetUserInfo()
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;
            return Json(new { Username = claimsIdentity.Name });
        }
        [HttpPost("[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _usersService.GetCurrentUserAsync();
            if (user == null)
            {
                return BadRequest("NotFound");
            }

            var (Succeeded, Error) = await _usersService.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (Succeeded)
            {
                return Ok();
            }

            return BadRequest(Error);
        }
    }
}
