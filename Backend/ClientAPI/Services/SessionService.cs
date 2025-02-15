using ClientAPI.Interfaces;
using Middleware_Components.DTO.ClientAPI;
using Middleware_Components.Services;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;

namespace ClientAPI.Services
{
    public class SessionService : ISessionService
    {
        private readonly ICacheService _cache;

        public SessionService(ICacheService cache) {
            _cache = cache;
        }

        public void SetupSession(Guid userGUID, string accessToken)
        {
            if (_cache.CheckExistKeysStorage<List<Session_Init>>(userGUID, "session_storage"))
            {

                var sessionList = _cache.GetKeyFromStorage<List<Session_Init>>(userGUID, "session_storage");

                foreach (var session in sessionList)
                {
                    if (session.statusSession == "active")
                    {
                        session.timeDel = DateTime.UtcNow;
                        session.statusSession = "expired";
                    }
                }

                sessionList.Add(new Session_Init()
                {
                    timeAdd = DateTime.UtcNow,
                    statusSession = "active",
                    tokenSession = accessToken
                });

                _cache.WriteKeyInStorage(userGUID, "session_storage", sessionList, DateTime.UtcNow.AddDays(7));
            }
            else
            {
                _cache.WriteKeyInStorage(userGUID, "session_storage", new List<Session_Init>()
                    {
                        new Session_Init()
                        {
                            timeAdd = DateTime.UtcNow,
                            statusSession = "active",
                            tokenSession = accessToken
                        }
                    }, DateTime.UtcNow.AddDays(7));
            }
        }

        public List<Session_Init>? GetSessions(Guid userGUID)
        {
            if (_cache.CheckExistKeysStorage<List<Session_Init>>(userGUID, "session_storage"))
            {
                var sessions = _cache.GetKeyFromStorage<List<Session_Init>>(userGUID, "session_storage");

                return sessions;
            }

            return null;
        }

        public void ClientSignOutSession(Guid userGUID)
        {
            if (_cache.CheckExistKeysStorage<List<Session_Init>>(userGUID, "session_storage"))
            {
                var sessionList = _cache.GetKeyFromStorage<List<Session_Init>>(userGUID, "session_storage");

                foreach (var session in sessionList)
                {
                    if (session.statusSession == "active")
                    {
                        session.timeDel = DateTime.UtcNow;
                        session.statusSession = "expired";
                    }
                }

                _cache.WriteKeyInStorage(userGUID, "session_storage", sessionList, DateTime.UtcNow.AddDays(7));
            }


        }

        public void RefreshSession(Guid userGUID, string accessToken)
        {
            if (_cache.CheckExistKeysStorage<List<Session_Init>>(userGUID, "session_storage"))
            {
                var sessionList = _cache.GetKeyFromStorage<List<Session_Init>>(userGUID, "session_storage");

                foreach (var session in sessionList)
                {
                    if (session.statusSession == "active")
                    {
                        session.timeUpd = DateTime.UtcNow;
                        session.tokenSession = accessToken;
                    }
                }

                _cache.WriteKeyInStorage(userGUID, "session_storage", sessionList, DateTime.UtcNow.AddDays(7));
            }
        }

        public void DeleteSession(Guid userGUID)
        {
            if (_cache.CheckExistKeysStorage<List<Session_Init>>(userGUID, "session_storage"))
                _cache.DeleteKeyFromStorage(userGUID, "session_storage");
        }
    }
}
