using Middleware_Components.DTO.ClientAPI;
using ORM_Components.DTO.ClientAPI;
using ORM_Components.DTO.ClientAPI.ClientsAll;

namespace ClientAPI.Interfaces
{
    public interface IClientService
    {
        public Task<Auth_PairTokens?> RegisterUser(AuthSignUp dtoObj);

        public Auth_PairTokens? LoginClient(AuthSignIn dtoObj);

        public Task<string?> ClientSignOut(string bearer_key);

        public Task<Auth_PairTokens?> RefreshClientSession(Auth_RefreshTokens dtoObj);

        public Task<ClientInfo?> ClientMeInfo(string bearer_key);

        public Task<ClientGetAll?> AllProfilesGet(string bearer_key, int from, int count);

        public Task UpdateClientInfo(string bearer_key, ClientUpdate dtoObj);

        public Task UpdateClientInfoWithAdmin(string bearer_key, ClientUpdate_Admin dtoObj, Guid userGUID);

        public Task CreateClientWithAdmin(string bearer_key, ClientAdd_Admin dtoObj);

        public Task DeleteClientWithAdmin(string bearer_key, Guid userGUID);
    }
}
