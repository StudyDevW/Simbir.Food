using Microsoft.EntityFrameworkCore;
using ORM_Components;
using ORM_Components.DTO.ClientAPI;
using ORM_Components.Tables;
using VK_Components.Interfaces;

namespace VK_Components.Services
{
    public class DatabaseOperations : IDatabaseOperations
    {
        private readonly DataContext _dbcontext;
        public DatabaseOperations(DataContext dbcontext) {
            _dbcontext = dbcontext;
        }

        private bool UserExist(long vk_id)
        {
            var checkUser = _dbcontext.userTable.Where(c => c.vk_id == vk_id).FirstOrDefault();

            if (checkUser != null) { return true; }

            return false;
        }

        public async Task AddUserFromVK(AuthAddUser dto, bool admin)
        {
            if (dto == null)
            {
                throw new Exception("dto null");
            }

            if (UserExist(dto.vk_id))
            {
                throw new Exception("user already exist");
            }

            UserTable usersTable = new UserTable()
            {
                vk_id = dto.vk_id,
                first_name = dto.first_name,
                last_name = dto.last_name,
                photo_url = dto.photo_max_orig,
                address = dto.address,
                roles = admin ? (dto.roles != null ? dto.roles : new string[] { "Client" }) : new string[] { "Client" }
            };

            _dbcontext.userTable.Add(usersTable);
            await _dbcontext.SaveChangesAsync();
        }
    }
}
