using Middleware_Components.JWT.DTO.CheckUsers;
using ORM_Components.DTO.ClientAPI;

namespace ClientAPI.Interfaces
{
    public interface IDatabaseService
    {
        public Task RegisterUser(AuthSignUp dto);

        public Auth_CheckInfo CheckUser(AuthSignIn dto);
    }
}
