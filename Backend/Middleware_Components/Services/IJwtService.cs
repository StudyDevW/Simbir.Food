using Middleware_Components.JWT.DTO.CheckUsers;
using Middleware_Components.JWT.DTO.Token;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Middleware_Components.Services
{
    public interface IJwtService
    {
        public string JwtTokenCreation(Auth_CheckSuccess dtoObj);

        public string RefreshTokenCreation(Auth_CheckSuccess dtoObj);

        // public Task<JwtSecurityToken> RSAJwtValidation(string? token);

        public Task<Token_ValidProperties> AccessTokenValidation(string? bearerKey, string checkrole = "none");

        public Task<Token_ValidProperties> RefreshTokenValidation(string? bearerKey);
    }
}
