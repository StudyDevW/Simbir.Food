using Middleware_Components.DTO.ClientAPI;

namespace ClientAPI.Interfaces
{
    public interface ISessionService
    {
        public void SetupSession(Guid userGUID, string accessToken);

        public List<Session_Init>? GetSessions(Guid userGUID);

        public void DeleteSession(Guid userGUID);
    }
}
