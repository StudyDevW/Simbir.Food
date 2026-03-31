using ORM_Components.DTO.ClientAPI;

namespace VK_Components.Interfaces
{
    public interface IDatabaseOperations
    {
        public Task AddUserFromVK(AuthAddUser dto, bool admin = false);
    }
}
