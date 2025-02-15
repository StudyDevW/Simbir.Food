using ClientAPI.Interfaces;
using Middleware_Components.DTO.ClientAPI;
using Middleware_Components.JWT.DTO.CheckUsers;
using Middleware_Components.Services;
using ORM_Components.DTO.ClientAPI;

namespace ClientAPI.Services
{
    public class ClientService : IClientService
    {
        private readonly IDatabaseService _database;
        private readonly ISessionService _session;
        private readonly IJwtService _jwt;
        private readonly ICacheService _cache;
        private readonly ILogger _logger;
        

        public ClientService(ISessionService session, IDatabaseService database, IJwtService jwt, ICacheService cache) {

            _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger("client-service-logger");
            _database = database;
            _jwt = jwt;
            _cache = cache;
            _session = session;
        }

        private Auth_PairTokens? TokensReleased(Auth_CheckInfo check)
        {
            if (check.check_success == null)
                return null;

            if (check.CheckHasSuccess())
            {
                var accessToken = _jwt.JwtTokenCreation(check.check_success);
                var refreshToken = _jwt.RefreshTokenCreation(check.check_success);

                if (_cache.CheckExistKeysStorage(check.check_success.Id, "accessTokens"))
                    _cache.DeleteKeyFromStorage(check.check_success.Id, "accessTokens");

                if (_cache.CheckExistKeysStorage(check.check_success.Id, "refreshTokens"))
                    _cache.DeleteKeyFromStorage(check.check_success.Id, "refreshTokens");

                _cache.WriteKeyInStorage(check.check_success.Id, "accessTokens", accessToken, DateTime.UtcNow.AddMinutes(5));
                _cache.WriteKeyInStorage(check.check_success.Id, "refreshTokens", refreshToken, DateTime.UtcNow.AddDays(7));

                _session.SetupSession(check.check_success.Id, accessToken);

                Auth_PairTokens pair_tokens = new Auth_PairTokens()
                {
                    accessToken = _cache.GetKeyFromStorage(check.check_success.Id, "accessTokens"),
                    refreshToken = _cache.GetKeyFromStorage(check.check_success.Id, "refreshTokens")
                };

                _logger.LogInformation($"Пользователь {check.check_success.Id} успешно вошел!");

                return pair_tokens;
            }

            return null;
        }

        public async Task<Auth_PairTokens?> RegisterUser(AuthSignUp dtoObj)
        {
            try
            {
                await _database.RegisterUser(dtoObj);

                _logger.LogInformation("Клиент успешно зарегистрировался");

                var check = _database.CheckUser(new AuthSignIn()
                {
                    login = dtoObj.login, 
                    password = dtoObj.password
                });

                var tokens = TokensReleased(check);

                if (tokens != null)
                    return tokens;

                _logger.LogInformation("Ошибка при авторизации");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            return null;
        }

        public Auth_PairTokens? LoginClient(AuthSignIn dtoObj)
        {
            var check = _database.CheckUser(dtoObj);

            if (check.CheckHasSuccess())
            {
                var tokens = TokensReleased(check);

                if (tokens != null)
                    return tokens;

                _logger.LogInformation("Ошибка при авторизации");
                return null;
            }

            return null;
        }



        public async Task<string?> ClientSignOut(string bearer_key)
        {
            var validation = await _jwt.AccessTokenValidation(bearer_key);

            if (validation.TokenHasError())
            {
                return null;
            }
            else if (validation.TokenHasSuccess())
            {
                _session.ClientSignOutSession(validation.token_success.Id);
      
                _cache.DeleteKeyFromStorage(validation.token_success.Id, "accessTokens");

                _cache.DeleteKeyFromStorage(validation.token_success.Id, "refreshTokens");

                _logger.LogInformation($"Пользователь id: {validation.token_success.Id} вышел!");

                return $"{validation.token_success.Id}_is_logout";
            }

            return null;
        }

        public async Task<Auth_PairTokens?> RefreshClientSession(Auth_RefreshTokens dtoObj)
        {
            var validation = await _jwt.RefreshTokenValidation(dtoObj.refreshToken);

            if (validation.TokenHasError())
            {
                return null;
            }
            else if (validation.TokenHasSuccess())
            {
                Auth_CheckSuccess authsuccess = new Auth_CheckSuccess()
                {
                    Id = validation.token_success.Id,
                    roles = validation.token_success.userRoles,
                    username = validation.token_success.userName
                };

                var accessToken = _jwt.JwtTokenCreation(authsuccess);
                var refreshToken = _jwt.RefreshTokenCreation(authsuccess);

                _session.RefreshSession(authsuccess.Id, accessToken);

                if (_cache.CheckExistKeysStorage(authsuccess.Id, "accessTokens"))
                    _cache.DeleteKeyFromStorage(authsuccess.Id, "accessTokens");

                if (_cache.CheckExistKeysStorage(authsuccess.Id, "refreshTokens"))
                    _cache.DeleteKeyFromStorage(authsuccess.Id, "refreshTokens");


                _cache.WriteKeyInStorage(authsuccess.Id, "accessTokens", accessToken, DateTime.UtcNow.AddMinutes(5));
                _cache.WriteKeyInStorage(authsuccess.Id, "refreshTokens", refreshToken, DateTime.UtcNow.AddDays(7));



                Auth_PairTokens pair_tokens = new Auth_PairTokens()
                {
                    accessToken = _cache.GetKeyFromStorage(authsuccess.Id, "accessTokens"),
                    refreshToken = _cache.GetKeyFromStorage(authsuccess.Id, "refreshTokens")
                };


                _logger.LogInformation($"Токены для id: {validation.token_success.Id} обновлены!");

                return pair_tokens;
            }

            return null;
        }

        public async Task<ClientInfo?> ClientMeInfo(string bearer_key)
        {
            var validation = await _jwt.AccessTokenValidation(bearer_key);

            if (validation.TokenHasError())
            {
                return null;
            }
            else if (validation.TokenHasSuccess())
            {
                var info_user = _database.InfoClientDatabase(validation.token_success.Id);

                if (info_user != null)
                    return info_user;
                else
                    return null;
            }

            return null;
        }

    }
}
