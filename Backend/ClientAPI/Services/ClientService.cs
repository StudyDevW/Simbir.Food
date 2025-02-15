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

            _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger("ClientAPI | client-service");
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
                if (_cache.CheckExistKeysStorage<List<Session_Init>>(validation.token_success.Id, "session_storage"))
                {
                    var sessionList = _cache.GetKeyFromStorage<List<Session_Init>>(validation.token_success.Id, "session_storage");

                    foreach (var session in sessionList)
                    {
                        if (session.statusSession == "active")
                        {
                            session.timeDel = DateTime.UtcNow;
                            session.statusSession = "expired";
                        }
                    }

                    _cache.WriteKeyInStorage(validation.token_success.Id, "session_storage", sessionList, DateTime.UtcNow.AddDays(7));
                }


                _cache.DeleteKeyFromStorage(validation.token_success.Id, "accessTokens");

                _cache.DeleteKeyFromStorage(validation.token_success.Id, "refreshTokens");

                _logger.LogInformation($"Пользователь id: {validation.token_success.Id} вышел!");

                return $"{validation.token_success.Id}_is_logout";
            }

            return null;
        }

    }
}
