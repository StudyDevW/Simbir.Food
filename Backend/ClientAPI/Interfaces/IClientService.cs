using Middleware_Components.DTO.ClientAPI;
using ORM_Components.DTO.ClientAPI;

namespace ClientAPI.Interfaces
{
    public interface IClientService
    {
        public Task<Auth_PairTokens?> RegisterUser(AuthSignUp dtoObj);

        public Auth_PairTokens? LoginClient(AuthSignIn dtoObj);

        public Task<string?> ClientSignOut(string bearer_key);
    }
}
