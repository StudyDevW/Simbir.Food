using Microsoft.EntityFrameworkCore;
using ORM_Components;
using ORM_Components.DTO.ClientAPI;
using ORM_Components.Tables;
using Telegram_Components.Interfaces;

namespace Telegram_Components.Services
{
    public class DatabaseOperations : IDatabaseOperations
    {
        private readonly DataContext _dbcontext;
        public DatabaseOperations(DataContext dbcontext) {
            _dbcontext = dbcontext;
        }

        private bool UserExist(long telegram_id)
        {
            var checkUser = _dbcontext.userTable.Where(c => c.telegram_id == telegram_id).FirstOrDefault();

            if (checkUser != null) { return true; }

            return false;
        }

        public async Task AddUserFromTelegram(AuthAddUser dto, bool admin)
        {
            //todo: test
            if (dto == null)
            {
                throw new Exception("dto null");
            }

            if (UserExist(dto.id))
            {
                throw new Exception("user already exist");
            }

            UserTable usersTable = new UserTable()
            {
                telegram_id = dto.id,
                telegram_chat_id = dto.chat_id,
                first_name = dto.first_name,
                last_name = dto.last_name,
                photo_url = dto.photo_url,
                username = dto.username,
                address = dto.address,
                roles = admin ? (dto.roles != null ? dto.roles : new string[] { "Client" }) : new string[] { "Client" }
            };

            _dbcontext.userTable.Add(usersTable);
            await _dbcontext.SaveChangesAsync();
        }
    }
}
