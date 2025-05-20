using Microsoft.AspNetCore.Identity;
using RepoPatternAndJwt.Core.Models.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepoPatternAndJwt.Core.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }


        // Refresh Token
        public List<RefreshToken>? refreshTokens { get; set; }
    }
}
