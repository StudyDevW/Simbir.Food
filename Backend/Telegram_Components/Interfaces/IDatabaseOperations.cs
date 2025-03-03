using ORM_Components.DTO.ClientAPI;

namespace Telegram_Components.Interfaces
{
    public interface IDatabaseOperations
    {
        public Task AddUserFromTelegram(AuthAddUser dto, bool admin = false);
    }
}
