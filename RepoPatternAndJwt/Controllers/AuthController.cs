using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RepoPatternAndJwt.Core.Models.Authentication;
using RepoPatternAndJwt.Core.RepositoriesInterFace;

namespace RepoPatternAndJwt.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthServices _authServices;
        public AuthController(IAuthServices authServices)
        {
            _authServices = authServices;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> RegisterAsync([FromForm] RegisterModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authServices.RegisterAsync(model);

            if (!result.IsAuthenticated)
                return BadRequest(result.Message);

            // If successful, generate a new Refresh Token and update it in the cookies
            SetRefreshTokenInCookies(result.RefreshToken, result.RefreshTokenExpiration);

            return Ok(
            new AuthModel
            {
                //expiresOn = result.ExpireOn,
                Email = result.Email,
                UserName = result.UserName,
                Roles = result.Roles,
                Token = result.Token,
                RefreshTokenExpiration = result.RefreshTokenExpiration,
            });
        }
        [HttpGet]
        [Authorize]
        public IActionResult Test()
        {
            return Content("Hello");
        }

        [HttpPost("Get-Token")]
        public async Task<IActionResult> GetTokenRequest([FromForm] TokenRequestModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authServices.GetTokenAsync(model);

            if (!result.IsAuthenticated)
                return BadRequest(result.Message);

            // Refresh Token
            if (!string.IsNullOrEmpty(result.RefreshToken))
            {
                SetRefreshTokenInCookies(result.RefreshToken, result.RefreshTokenExpiration);
            }

            return Ok(
            new
            {
                //expiresOn = result.ExpireOn,
                Email = result.Email,
                UserName = result.UserName,
                Roles = result.Roles,
                Token = result.Token,
                RefreshTokenExpiration = result.RefreshTokenExpiration,
            });
        }

        //Add Refresh Token To Cookies
        // Method to set the Refresh Token in cookies
        private void SetRefreshTokenInCookies(string refreshToken, DateTime expire)
        {
            // Define cookie options to configure how the cookie is stored
            var cookiesOption = new CookieOptions
            {
                // Set the cookie to be accessible only via HTTP requests (more secure)
                HttpOnly = true,
                // Set the expiration time of the cookie
                Expires = expire.ToLocalTime()
            };
            // Append the Refresh Token to the response cookies with the specified options
            Response.Cookies.Append("RefreshToken", refreshToken, cookiesOption);
        }

        // Endpoint to handle the Refresh Token process
        [HttpGet("Refresh-Token")]
        public async Task<IActionResult> RefreshToken()
        {
            // Retrieve the Refresh Token from the cookies sent with the request
            var refreshToken = Request.Cookies["RefreshToken"];

            // Call the service layer to validate and refresh the token
            var result = await _authServices.RefreshTokenAsunc(refreshToken);

            // If the Refresh Token is invalid or not authenticated, return an error response
            if (!result.IsAuthenticated)
                return BadRequest(result);

            // If successful, generate a new Refresh Token and update it in the cookies
            SetRefreshTokenInCookies(result.RefreshToken, result.RefreshTokenExpiration);

            // Return the updated authentication model (including the new tokens)
            return Ok(result);
        }

        [HttpPost("Revoked-Token")]
        public async Task<IActionResult> RevokToken([FromBody] RevokeToken model)
        {
            var token = model.Token ?? Request.Cookies["RefreshToken"];

            if (string.IsNullOrEmpty(token))
                return BadRequest("Token Is Required");

            var result = await _authServices.RevokedTokenAsunc(token);
            
            if(!result)
                return BadRequest("Token InVaild");

            return Ok("Token Revoked");
        }
    }
}
