using RepoPatternAndJwt.Core.Models.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepoPatternAndJwt.Core.RepositoriesInterFace
{
    public interface IAuthServices
    {
        Task<AuthModel> RegisterAsync(RegisterModel model);

        Task<AuthModel> GetTokenAsync(TokenRequestModel model);

        Task<AuthModel> RefreshTokenAsunc(string token);

        Task<bool> RevokedTokenAsunc(string token);
    }
}
