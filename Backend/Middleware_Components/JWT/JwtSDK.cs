using Middleware_Components.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Middleware_Components.JWT.DTO.Token;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using Middleware_Components.JWT.DTO.CheckUsers;

namespace Middleware_Components.JWT
{
    public class JwtSDK : IJwtService
    {
        private readonly IConfiguration _configuration;
        private readonly ICacheService _cache;
        public JwtSDK() { }

        public JwtSDK(IConfiguration conf, ICacheService cache)
        {
            _configuration = conf;
            _cache = cache;
        }

        private async Task<JwtSecurityToken> RSAJwtValidation(string? token)
        {
            var rsa = RSA.Create();

            rsa.ImportFromPem(_configuration["RSA_PUBLIC_KEY"]);

            RsaSecurityKey issuerSigningKey = new RsaSecurityKey(rsa);

            TokenValidationParameters tk_valid = new TokenValidationParameters
            {
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidAudience = _configuration["Jwt:Audience"],
                IssuerSigningKey = issuerSigningKey,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                RequireSignedTokens = true,
                RequireExpirationTime = false,
                ValidateLifetime = false
            };

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();

                tokenHandler.ValidateToken(token, tk_valid, out var rawValidatedToken);

                return (JwtSecurityToken)rawValidatedToken;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        private async Task<JwtSecurityToken> RSARefreshTokenValidation(string? token)
        {
            var rsa = RSA.Create();

            rsa.ImportFromPem(_configuration["RSA_PUBLIC_KEY"]);

            RsaSecurityKey issuerSigningKey = new RsaSecurityKey(rsa);

            TokenValidationParameters tk_valid = new TokenValidationParameters
            {
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidAudience = _configuration["Jwt:Audience"],
                IssuerSigningKey = issuerSigningKey,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                RequireSignedTokens = true,
                RequireExpirationTime = false,
                ValidateLifetime = false
            };

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();

                tokenHandler.ValidateToken(token, tk_valid, out var rawValidatedToken);

                return (JwtSecurityToken)rawValidatedToken;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public async Task<Token_ValidProperties> AccessTokenValidation(string? bearerKey, string checkrole)
        {

            if (bearerKey == null)
                return new Token_ValidProperties() { token_error = new Token_ValidError { errorLog = "token_empty" } };

            if (!bearerKey.Contains("Bearer "))
                return new Token_ValidProperties() { token_error = new Token_ValidError { errorLog = "format_unknown" } };

            string bearer_key_without_prefix = bearerKey.Substring("Bearer ".Length);

            var validation = await RSAJwtValidation(bearer_key_without_prefix);

            var expectedAlg = SecurityAlgorithms.RsaSha512;

            if (validation == null)
            {
                return new Token_ValidProperties() { token_error = new Token_ValidError { errorLog = "unauthorized" } };
            }
            else
            {
                if (validation.Header?.Alg == null || validation.Header?.Alg != expectedAlg)
                {
                    return new Token_ValidProperties() { token_error = new Token_ValidError { errorLog = "unexpected_alg" } };
                }

                string device = "";
                Guid userGUID = Guid.Empty;
                List<string> userRoles = new List<string>();

                foreach (var claim in validation.Claims)
                {
                    if (claim.Type == "Id")
                        userGUID = Guid.Parse(claim.Value);

                    if (claim.Type == "Device")
                        device = claim.Value;

                    if (claim.Type == "Roles")
                        userRoles = JsonSerializer.Deserialize<List<string>>(claim.Value);
                }

                if (userGUID == Guid.Empty)
                    return new Token_ValidProperties() { token_error = new Token_ValidError { errorLog = "unauthorized" } };

                if (userRoles == null)
                    return new Token_ValidProperties() { token_error = new Token_ValidError { errorLog = "unauthorized" } };

                if (_cache.CheckExistKeysStorage(userGUID, "accessTokens"))
                {
                    //Проверка на то, подменен ли ключ или нет!
                    if (_cache.GetKeyFromStorage(userGUID, "accessTokens") != bearer_key_without_prefix)
                        return new Token_ValidProperties() { token_error = new Token_ValidError { errorLog = "unauthorized" } };
                }
                else
                    return new Token_ValidProperties() { token_error = new Token_ValidError { errorLog = "unauthorized" } };

                if (checkrole != "none")
                {
                    if (!userRoles.Contains(checkrole))
                        return new Token_ValidProperties() { token_error = new Token_ValidError { errorLog = "unauthorized_role" } };
                }

                Token_ValidSuccess valid_success = new Token_ValidSuccess
                {
                    Id = userGUID,
                    userRoles = userRoles,
                    deviceInfo = device,
                    bearerWithoutPrefix = bearer_key_without_prefix
                };

                return new Token_ValidProperties()
                {
                    token_success = valid_success
                };
            }
        }

        public async Task<Token_ValidProperties> RefreshTokenValidation(string? bearerKey)
        {
            var validation = await RSARefreshTokenValidation(bearerKey);

            var expectedAlg = SecurityAlgorithms.RsaSha512;

            if (validation == null)
            {
                return new Token_ValidProperties() { token_error = new Token_ValidError { errorLog = "unauthorized" } };
            }
            else
            {
                if (validation.Header?.Alg == null || validation.Header?.Alg != expectedAlg)
                {
                    return new Token_ValidProperties() { token_error = new Token_ValidError { errorLog = "unexpected_alg" } };
                }

               
                string device = "";
                Guid userGUID = Guid.Empty;
                List<string> userRoles = new List<string>();

                foreach (var claim in validation.Claims)
                {
                    if (claim.Type == "Id")
                        userGUID = Guid.Parse(claim.Value);

                    if (claim.Type == "Device")
                        device = claim.Value;

                    if (claim.Type == "Roles")
                        userRoles = JsonSerializer.Deserialize<List<string>>(claim.Value);
                }

                if (userGUID == Guid.Empty)
                    return new Token_ValidProperties() { token_error = new Token_ValidError { errorLog = "unauthorized" } };

                if (userRoles == null)
                    return new Token_ValidProperties() { token_error = new Token_ValidError { errorLog = "unauthorized" } };


                if (_cache.CheckExistKeysStorage(userGUID, "refreshTokens"))
                {
                    //Проверка на то, подменен ли ключ или нет!
                    if (_cache.GetKeyFromStorage(userGUID, "refreshTokens") != bearerKey)
                        return new Token_ValidProperties() { token_error = new Token_ValidError { errorLog = "unauthorized" } };
                }
                else
                    return new Token_ValidProperties() { token_error = new Token_ValidError { errorLog = "unauthorized" } };

                Token_ValidSuccess valid_success = new Token_ValidSuccess
                {
                    Id = userGUID,
                    userRoles = userRoles,
                    deviceInfo = device,
                    bearerWithoutPrefix = bearerKey
                };

                return new Token_ValidProperties()
                {
                    token_success = valid_success
                };
            }
        }

        public string JwtTokenCreation(Auth_CheckSuccess dtoObj)
        {
            if (dtoObj == null)
                return string.Empty;

            if (dtoObj.device == null)
                return string.Empty;

            var rsaprivateKey = _configuration["RSA_PRIVATE_KEY"];

            using var rsa = RSA.Create();
            rsa.ImportFromPem(rsaprivateKey);

            var signingCredentials = new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha512)
            {
                CryptoProviderFactory = new CryptoProviderFactory { CacheSignatureProviders = false }
            };

            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            var serializer_roles = JsonSerializer.Serialize(dtoObj.roles);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("Id", dtoObj.Id.ToString()),
                    new Claim("Device", dtoObj.device),
                    new Claim("Roles", serializer_roles),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                }),
                Audience = audience,
                Issuer = issuer,
                SigningCredentials = signingCredentials,

            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);

            var jwtToken = jwtTokenHandler.WriteToken(token);

            return jwtToken;
        }

        public string RefreshTokenCreation(Auth_CheckSuccess dtoObj)
        {
            if (dtoObj == null)
                return string.Empty;

            if (dtoObj.device == null)
                return string.Empty;

            var rsaprivateKey = _configuration["RSA_PRIVATE_KEY"];

            using var rsa = RSA.Create();
            rsa.ImportFromPem(rsaprivateKey);

            var signingCredentials = new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha512)
            {
                CryptoProviderFactory = new CryptoProviderFactory { CacheSignatureProviders = false }
            };

            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            var serializer_roles = JsonSerializer.Serialize(dtoObj.roles);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("Id", dtoObj.Id.ToString()),
                    new Claim("Device", dtoObj.device),
                    new Claim("Roles", serializer_roles),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                }),
                Audience = audience,
                Issuer = issuer,
                SigningCredentials = signingCredentials
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);

            var jwtToken = jwtTokenHandler.WriteToken(token);

            return jwtToken;
        }
    }
}
