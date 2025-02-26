using ORM_Components.DTO.ClientAPI;
using ORM_Components.Tables;
using Microsoft.AspNetCore.Identity;
using ORM_Components;
using ClientAPI.Interfaces;
using Middleware_Components.JWT.DTO.CheckUsers;
using ORM_Components.DTO.ClientAPI.ClientsAll;

namespace ClientAPI.Services
{
    public class DatabaseService : IDatabaseService
    {
        private readonly ILogger _logger;
        private readonly DataContext _dbcontext;
        private readonly PasswordHasher<PasswordAppUser> _passwordHasher;

        public DatabaseService(DataContext dbcontext)
        {
            _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger("database-service-logger");
            _passwordHasher = new PasswordHasher<PasswordAppUser>();
            _dbcontext = dbcontext;
        }

        public async Task UserUpdateFromTelegram(ClientUpdate dtoObj)
        {
            var selectedUser = _dbcontext.userTable.Where(c => c.telegram_id == dtoObj.id).FirstOrDefault();

            if (selectedUser == null)
                throw new Exception("user_not_found");

            if (selectedUser.first_name != dtoObj.first_name)
            {
                selectedUser.first_name = dtoObj.first_name;
                await _dbcontext.SaveChangesAsync();
            }

            if (selectedUser.last_name != dtoObj.last_name && dtoObj.last_name != null)
            {
                selectedUser.address = dtoObj.address;
                await _dbcontext.SaveChangesAsync();
            }

            if (selectedUser.username != dtoObj.username && dtoObj.username != null)
            {
                selectedUser.username = dtoObj.username;
                await _dbcontext.SaveChangesAsync();
            }

            if (selectedUser.address != dtoObj.address && dtoObj.address != null && dtoObj.address != "")
            {
                selectedUser.address = dtoObj.address;
                await _dbcontext.SaveChangesAsync();
            }

            if (selectedUser.photo_url != dtoObj.photo_url && dtoObj.photo_url != null)
            {
                selectedUser.photo_url = dtoObj.photo_url;
                await _dbcontext.SaveChangesAsync();
            }

        }


        public Auth_CheckInfo CheckUser(AuthSignIn dto)
        {
            if (dto == null)
            {
                _logger.LogError("CheckUser: dto==null");
                return new Auth_CheckInfo() { check_error = new Auth_CheckError { errorLog = "input_incorrect" } };
            }

            var userFound = _dbcontext.userTable.Where(
                c => c.telegram_chat_id == dto.telegram_chat_id
            ).FirstOrDefault();

            if (userFound != null)
            {
                return new Auth_CheckInfo()
                {
                    check_success = new Auth_CheckSuccess
                    {
                        Id = userFound.Id,
                        device = dto.device,
                        telegram_chat_id = dto.telegram_chat_id,
                        roles = userFound.roles.ToList()
                    }
                };
            }

            return new Auth_CheckInfo() { check_error = new Auth_CheckError { errorLog = "error_found" } };
        }

        public ClientInfo? InfoClientDatabase(Guid userGUID)
        {
            var selectedUser = _dbcontext.userTable.Where(c => c.Id  == userGUID).FirstOrDefault();

            if (selectedUser != null)
            {
                _logger.LogInformation($"InfoClientDatabase: Запрошена информация о аккаунте (id: {userGUID})");

                return new ClientInfo()
                {
                    Id = selectedUser.Id,
                    telegram_id = selectedUser.telegram_id,
                    chat_id = selectedUser.telegram_chat_id,
                    first_name = selectedUser.first_name,
                    last_name = selectedUser.last_name,
                    username = selectedUser.username,
                    address = selectedUser.address,
                    photo_url = selectedUser.photo_url,
                    roles = selectedUser.roles.ToList()
                };
            }

            return null;
        }

        public ClientGetAll GetAllClients(int _from, int _count)
        {
            ClientGetAll allClients = new ClientGetAll();

            allClients.Settings = new ClientSelectionSettings { from = _from, count = _count };

            List<ClientInfo> clients = new List<ClientInfo>();

            if (_count != 0)
            {
                var filteredQuery = _dbcontext.userTable.Skip(_from).Take(_count);

                foreach (var client in filteredQuery)
                {
                    ClientInfo clientInfo = new ClientInfo()
                    {
                        Id = client.Id,
                        telegram_id = client.telegram_id,
                        chat_id = client.telegram_chat_id,
                        first_name = client.first_name,
                        last_name = client.last_name,
                        username = client.username,
                        address = client.address,
                        photo_url = client.photo_url,
                        roles = client.roles.ToList()
                    };

                    clients.Add(clientInfo);
                }
            }
            else
            {
                var filteredQuery = _dbcontext.userTable.Skip(_from);

                foreach (var client in filteredQuery)
                {
                    ClientInfo clientInfo = new ClientInfo()
                    {
                        Id = client.Id,
                        telegram_id = client.telegram_id,
                        chat_id = client.telegram_chat_id,
                        first_name = client.first_name,
                        last_name = client.last_name,
                        username = client.username,
                        address = client.address,
                        photo_url = client.photo_url,
                        roles = client.roles.ToList()
                    };

                    clients.Add(clientInfo);
                }
            }

            allClients.ContentFill(clients);

            return allClients;
        }

        private async Task DeleteAllFoodItems(Guid restaurantId)
        {
            var selectedItems = _dbcontext.restaurantFoodItemsTable.Where(c => c.restaurant_id == restaurantId).ToList();

            if (selectedItems != null)
            {
                foreach (var item in selectedItems)
                {
                    _dbcontext.restaurantFoodItemsTable.Remove(item);
                    await _dbcontext.SaveChangesAsync();
                }
            }
        }

        private async Task DeleteAllRestaurants(Guid id)
        {
            var selectedRestaurant = _dbcontext.restaurantTable.Where(c => c.user_id == id).ToList();

            if (selectedRestaurant != null)
            {
                foreach (var restaurant in selectedRestaurant)
                {
                    _dbcontext.restaurantTable.Remove(restaurant);
                    await _dbcontext.SaveChangesAsync();

                    await DeleteAllFoodItems(restaurant.Id);
                }
            }
        }

        private async Task DeleteCourierStatus(Guid id)
        {
            var selectedCourier = _dbcontext.courierTable.Where(c => c.userId == id).FirstOrDefault();

            if (selectedCourier != null)
            {
                _dbcontext.courierTable.Remove(selectedCourier);
                await _dbcontext.SaveChangesAsync();
            }
        }

        public async Task DeleteClientWithAdmin(Guid id)
        {
            var selectedUser = _dbcontext.userTable.Where(c => c.Id == id).FirstOrDefault();

            if (selectedUser == null)
                throw new Exception("user_not_found");

            _dbcontext.userTable.Remove(selectedUser);
            await _dbcontext.SaveChangesAsync();

            //Если привязаны рестораны, удаляем рестораны
            await DeleteAllRestaurants(id);

            //Если привязан курьер, удаляем курьера
            await DeleteCourierStatus(id);

            _logger.LogInformation($"DeleteClientWithAdmin: (id: {id} ) был удален");
        }

    }
}
