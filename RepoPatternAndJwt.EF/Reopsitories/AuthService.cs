using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RepoPatternAndJwt.Core.Helper;
using RepoPatternAndJwt.Core.Models.Authentication;
using RepoPatternAndJwt.Core.Models;
using RepoPatternAndJwt.Core.RepositoriesInterFace;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;

namespace RepoPatternAndJwt.EF.Reopsitories
{
    public class AuthServices : IAuthServices
    {
        // The UserManager<TUser> class is used to manage user information.
        // The TUser parameter is typically an instance of a class that extends IdentityUser.
        // This class provides methods for creating, deleting, updating, and retrieving user information
        // and managing user-related features like changing passwords, assigning roles, and claims.
        private readonly UserManager<ApplicationUser> _userManager;


        private readonly JwtSetting _jwt;
        // The IOptions<T> interface allows accessing a snapshot of the configuration settings as a strongly
        // typed object. It's typically used for reading configuration options at application program.cs
        // and provides a way to access the options in a typed manner
        // builder.Services.Configure<JwtSetting>(builder.Configuration.GetSection("JWT")); in program.cs

        public AuthServices(UserManager<ApplicationUser> userManager, IOptions<JwtSetting> jwt)
        {
            _userManager = userManager;
            _jwt = jwt.Value;
        }

        public async Task<AuthModel> GetTokenAsync(TokenRequestModel model)
        {
            var authModel = new AuthModel();

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                authModel.Message = "Email Or Password is Incorrect";
                return authModel;
            }

            var jwtSecurityToken = await CreateJwtToken(user);
            var roleList = await _userManager.GetRolesAsync(user);

            authModel.IsAuthenticated = true;
            authModel.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            authModel.Email = model.Email;
            //authModel.ExpireOn = jwtSecurityToken.ValidTo;
            authModel.Roles = roleList.ToList();
            if (user.refreshTokens.Any(t => t.IsActive))
            {
                var activeRefreshToken = user.refreshTokens.FirstOrDefault(t=> t.IsActive);
                authModel.RefreshToken = activeRefreshToken.token;
                authModel.RefreshTokenExpiration = activeRefreshToken.ExpireOn;
            }else
            {
                var newRefreshToken = GetRefreshToken();
                authModel.RefreshToken = newRefreshToken.token;
                authModel.RefreshTokenExpiration = newRefreshToken.ExpireOn;
                user.refreshTokens.Add(newRefreshToken);
                await _userManager.UpdateAsync(user);
            }

            return authModel;
        }

        public async Task<AuthModel> RegisterAsync(RegisterModel model)
        {
            if (await _userManager.FindByEmailAsync(model.Email) is not null)
                return new AuthModel { Message = "Email Is Already Exist" };

            if (await _userManager.FindByNameAsync(model.UserName) is not null)
                return new AuthModel { Message = "UserName Is Already Exist" };

            var user = new ApplicationUser
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                UserName = model.UserName,
            };

            //Add User And Make Hash to Password
            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                var errorList = string.Empty;
                foreach (var error in result.Errors)
                {
                    errorList += $"{error.Description}\n";
                }
                return new AuthModel { Message = errorList };
            }

            //Add To Role
            await _userManager.AddToRoleAsync(user, ApplicationRoles.UserRole);

            var jwtSecurityToken = await CreateJwtToken(user);
            return new AuthModel
            {
                UserName = user.UserName,
                Email = user.Email,
                //ExpireOn = jwtSecurityToken.ValidTo,
                IsAuthenticated = true,
                Roles = new List<string> { ApplicationRoles.UserRole },
                Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken)
            };

        }

        private async Task<JwtSecurityToken> CreateJwtToken(ApplicationUser user)
        {
            // This line is calling a method to get the claims for the given user
            var userClaim = await _userManager.GetClaimsAsync(user);
            var userRole = await _userManager.GetRolesAsync(user);

            // Claims in the context of authentication are pieces of information about the user
            var roleClaim = new List<Claim>();

            foreach (var role in userRole)
            {
                roleClaim.Add(new Claim("role", role));
            }
            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserName),  
            // Sub: The subject of the token, usually the username or user identifier
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),  
            // Jti: JWT ID (unique identifier for the token)
            new Claim(JwtRegisteredClaimNames.Email, user.Email),  
            // Email: The user's email address
            new Claim("uid", user.Id)  
            // Custom claim "uid" to store the user's ID
        }
            .Union(userClaim)
            .Union(roleClaim);

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);
            // SigningCredentials is used to define how the token will be signed to ensure its integrity and authenticity

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(_jwt.DurationInMiuntes),
                signingCredentials: signingCredentials
                );


            return jwtSecurityToken;
        }

        public async Task<AuthModel> RefreshTokenAsunc(string token)
        {
            // Initialize an empty authentication model
            var authModel = new AuthModel();

            // Find the user associated with the given refresh token
            var user = await _userManager.Users.SingleOrDefaultAsync(u => u.refreshTokens.Any(t => t.token == token));

            // If no user is found with this token, return an error message
            if (user == null)
            {
                authModel.Message = "Invalid Token";
                return authModel;
            }

            // Retrieve the specific refresh token from the user's list of tokens
            var refreshToken = user.refreshTokens.Single(t => t.token == token);

            // If the token is inactive or doesn't exist, return an error message
            if (!refreshToken.IsActive)
            {
                authModel.Message = "Inactive Token";
                return authModel;
            }

            // Mark the old refresh token as revoked by setting the RevokedOn timestamp
            refreshToken.RevokedOn = DateTime.UtcNow;

            // Generate a new refresh token
            var newRefreshToken = GetRefreshToken();

            // Add the new refresh token to the user's list of tokens
            user.refreshTokens.Add(newRefreshToken);

            // Update the user in the database to save the new token
            await _userManager.UpdateAsync(user);

            // Generate a new JWT access token for the user
            var jwtToken = await CreateJwtToken(user);

            // Populate the authentication model with the new tokens and user information
            authModel.IsAuthenticated = true;
            authModel.Token = new JwtSecurityTokenHandler().WriteToken(jwtToken); // New JWT token
            authModel.Email = user.Email; // User's email
            authModel.UserName = user.UserName; // User's username

            // Retrieve the user's roles and add them to the authentication model
            var roles = await _userManager.GetRolesAsync(user);
            authModel.Roles = roles.ToList();

            // Add the new refresh token and its expiration date to the authentication model
            authModel.RefreshToken = newRefreshToken.token;
            authModel.RefreshTokenExpiration = newRefreshToken.ExpireOn;

            // Return the updated authentication model
            return authModel;
        }


        public async Task<bool> RevokedTokenAsunc(string token)
        {
            // Initialize an empty authentication model
            var authModel = new AuthModel();

            // Attempt to find the user associated with the given refresh token
            var user = await _userManager.Users.SingleOrDefaultAsync(u => u.refreshTokens.Any(t => t.token == token));

            // If no user is found associated with the provided token, return false
            if (user == null)
                return false;

            // Retrieve the specific refresh token from the user's list of tokens
            var refreshToken = user.refreshTokens.Single(t => t.token == token);

            // If the refresh token is inactive or doesn't exist, return false
            if (!refreshToken.IsActive)
                return false;

            // Mark the refresh token as revoked by setting the RevokedOn timestamp to the current UTC time
            refreshToken.RevokedOn = DateTime.UtcNow;

            // Update the user's refresh tokens in the database to reflect the revocation
            await _userManager.UpdateAsync(user);

            // Return true indicating the token has been successfully revoked
            return true;
        }

        private RefreshToken GetRefreshToken()
        {
            // Create a byte array to hold the random token value
            var randomNumber = new byte[32];

            // Use a cryptographic random number generator to fill the array
            using var generator = new RNGCryptoServiceProvider();
            generator.GetBytes(randomNumber);

            // Return a new RefreshToken object
            return new RefreshToken
            {
                // Convert the random byte array to a Base64-encoded string
                token = Convert.ToBase64String(randomNumber),
                // Set the creation time to the current UTC time
                CreatedOn = DateTime.UtcNow,
                // Set the expiration time to 7 days from now
                ExpireOn = DateTime.UtcNow.AddDays(7)
            };
        }

    }
}


