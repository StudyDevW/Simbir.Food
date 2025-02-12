using ORM_Components.DTO.CheckUsers;
using ORM_Components.DTO.ClientAPI;

namespace ClientAPI.Services
{
    public interface IDatabaseService
    {
        public Task RegisterUser(AuthSignUp dto);

        public Auth_CheckInfo CheckUser(AuthSignIn dto);
    }
}
