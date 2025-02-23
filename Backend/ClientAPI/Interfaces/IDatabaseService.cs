using Middleware_Components.JWT.DTO.CheckUsers;
using ORM_Components.DTO.ClientAPI;
using ORM_Components.DTO.ClientAPI.ClientsAll;

namespace ClientAPI.Interfaces
{
    public interface IDatabaseService
    {
        public Task UserUpdateFromTelegram(ClientUpdate dtoObj);

        public Auth_CheckInfo CheckUser(AuthSignIn dto);

        public ClientInfo? InfoClientDatabase(Guid userGUID);

        public ClientGetAll GetAllClients(int _from, int _count);

        public Task DeleteClientWithAdmin(Guid id);
    }
}
