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

        private bool UserExist(string login)
        {
            var checkUser = _dbcontext.userTable.Where(c => c.login == login).FirstOrDefault();

            if (checkUser != null) { return true; }

            return false;
        }

        public async Task RegisterUser(AuthSignUp dto)
        {
            if (dto == null)
            {
                throw new Exception("dto null");
            }

            if (UserExist(dto.login))
            {
                throw new Exception("login already exist");
            }

            PasswordAppUser passwordUser = new PasswordAppUser() { login = dto.login };

            passwordUser.passwordHashed = _passwordHasher.HashPassword(passwordUser, dto.password);


            UserTable usersTable = new UserTable()
            {
                name = dto.name,
                email = dto.email,
                login = dto.login,
                address = dto.address,
                phone_number = dto.phone_number,
                password = passwordUser.passwordHashed,
                roles = new string[] { "Client" },
            
            };

            _dbcontext.userTable.Add(usersTable);
            await _dbcontext.SaveChangesAsync();

            _logger.LogInformation($"RegisterUser: {dto.login}, создан");

        }

        public async Task RegisterUserWithAdmin(ClientAdd_Admin dto)
        {
            if (dto == null)
            {
                throw new Exception("dto null");
            }

            if (UserExist(dto.login))
            {
                throw new Exception("login already exist");
            }

            PasswordAppUser passwordUser = new PasswordAppUser() { login = dto.login };

            passwordUser.passwordHashed = _passwordHasher.HashPassword(passwordUser, dto.password);


            UserTable usersTable = new UserTable()
            {
                name = dto.name,
                email = dto.email,
                login = dto.login,
                address = dto.address,
                phone_number = dto.phone_number,
                password = passwordUser.passwordHashed,
                roles = dto.roles
            };

            _dbcontext.userTable.Add(usersTable);
            await _dbcontext.SaveChangesAsync();

            _logger.LogInformation($"RegisterUserWithAdmin: {dto.login}, создан");
        }

        private bool VerifyUserPassword(PasswordAppUser user, string password)
        {
            var result = _passwordHasher.VerifyHashedPassword(user, user.passwordHashed, password);
            return result == PasswordVerificationResult.Success;
        }

        public Auth_CheckInfo CheckUser(AuthSignIn dto)
        {
            if (dto == null)
            {
                _logger.LogError("CheckUser: dto==null");
                return new Auth_CheckInfo() { check_error = new Auth_CheckError { errorLog = "input_incorrect" } };
            }

            var userFound = _dbcontext.userTable.Where(
                c => c.login == dto.login
            ).FirstOrDefault();

            if (userFound != null)
            {

                var passVerify = VerifyUserPassword(new PasswordAppUser()
                {
                    login = dto.login,
                    passwordHashed = userFound.password
                }, dto.password);

                if (passVerify)
                    return new Auth_CheckInfo()
                    {

                        check_success = new Auth_CheckSuccess
                        {
                            Id = userFound.Id,
                            login = userFound.login,
                            telegramChatId = dto.telegram_chatid,
                            roles = userFound.roles.ToList()
                        }
                    };
            }


            _logger.LogError("CheckUser: Пользователь ввел неверно имя или пароль!");
            return new Auth_CheckInfo() { check_error = new Auth_CheckError { errorLog = "username/password_incorrect" } };
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
                    address = selectedUser.address,
                    name = selectedUser.name,
                    phone_number = selectedUser.phone_number,
                    email = selectedUser.email,
                    avatarImage = selectedUser.avatarImage,
                    login = selectedUser.login,
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
                        address = client.address,
                        avatarImage = client.avatarImage,
                        email = client.email,
                        login = client.login,
                        name = client.name,
                        phone_number = client.phone_number, 
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
                        address = client.address,
                        avatarImage = client.avatarImage,
                        email = client.email,
                        login = client.login,
                        name = client.name,
                        phone_number = client.phone_number,
                        roles = client.roles.ToList()
                    };

                    clients.Add(clientInfo);
                }
            }

            allClients.ContentFill(clients);

            return allClients;
        }

        public async Task InfoClientUpdate(ClientUpdate dtoObj, Guid userGUID)
        {
            var selectedUser = _dbcontext.userTable.Where(c => c.Id == userGUID).FirstOrDefault();

            if (selectedUser == null)
                throw new Exception("user_not_found");

            //Смена пароля
            if (dtoObj.password != null)
            {
                var passVerify = VerifyUserPassword(new PasswordAppUser()
                {
                    login = selectedUser.login,
                    passwordHashed = selectedUser.password
                }, dtoObj.password);

                if (passVerify) //Пароль один и тот же
                    throw new Exception("password_1:1");
                else
                {
                    PasswordAppUser passwordUser = new PasswordAppUser() { login = selectedUser.login };
                    passwordUser.passwordHashed = _passwordHasher.HashPassword(passwordUser, dtoObj.password);

                    selectedUser.password = passwordUser.passwordHashed;
                    await _dbcontext.SaveChangesAsync();
                }
            }

            //Смена имени
            if (selectedUser.name != dtoObj.name && dtoObj.name != null)
            {
                selectedUser.name = dtoObj.name;
                await _dbcontext.SaveChangesAsync();
            }

            //Смена почты
            if (selectedUser.email != dtoObj.email && dtoObj.email != null)
            {
                selectedUser.email = dtoObj.email;
                await _dbcontext.SaveChangesAsync();
            }

            //Смена номера телефона
            if (selectedUser.phone_number != dtoObj.phone_number && dtoObj.phone_number != null)
            {
                selectedUser.phone_number = dtoObj.phone_number;
                await _dbcontext.SaveChangesAsync();
            }

            //Смена адреса
            if (selectedUser.address != dtoObj.address && dtoObj.address != null)
            {
                selectedUser.address = dtoObj.address;
                await _dbcontext.SaveChangesAsync();
            }

            //Смена аватара
            if (selectedUser.avatarImage != dtoObj.avatarImage && dtoObj.avatarImage != null)
            {
                selectedUser.avatarImage = dtoObj.avatarImage;
                await _dbcontext.SaveChangesAsync();
            }

           
        }

        public async Task InfoClientUpdateWithAdmin(ClientUpdate_Admin dtoObj, Guid userGUID)
        {
            var selectedUser = _dbcontext.userTable.Where(c => c.Id == userGUID).FirstOrDefault();

            if (selectedUser == null)
                throw new Exception("user_not_found");

            //Смена имени
            if (selectedUser.name != dtoObj.name && dtoObj.name != null)
            {
                selectedUser.name = dtoObj.name;
                await _dbcontext.SaveChangesAsync();
            }

            //Смена логина или никнейма
            if (selectedUser.login != dtoObj.login && dtoObj.login != null)
            {
                selectedUser.login = dtoObj.login;
                await _dbcontext.SaveChangesAsync();
            }

            //Смена почты
            if (selectedUser.email != dtoObj.email && dtoObj.email != null)
            {
                selectedUser.email = dtoObj.email;
                await _dbcontext.SaveChangesAsync();
            }

            //Смена номера телефона
            if (selectedUser.phone_number != dtoObj.phone_number && dtoObj.phone_number != null)
            {
                selectedUser.phone_number = dtoObj.phone_number;
                await _dbcontext.SaveChangesAsync();
            }

            //Смена адреса
            if (selectedUser.address != dtoObj.address && dtoObj.address != null)
            {
                selectedUser.address = dtoObj.address;
                await _dbcontext.SaveChangesAsync();
            }

            //Смена аватара
            if (selectedUser.avatarImage != dtoObj.avatarImage && dtoObj.avatarImage != null)
            {
                selectedUser.avatarImage = dtoObj.avatarImage;
                await _dbcontext.SaveChangesAsync();
            }

            //Смена ролей
            if (selectedUser.roles != dtoObj.roles && dtoObj.roles != null)
            {
                selectedUser.roles = dtoObj.roles;
                await _dbcontext.SaveChangesAsync();
            }

            //Смена пароля
            if (dtoObj.password != null)
            {
                var passVerify = VerifyUserPassword(new PasswordAppUser()
                {
                    login = selectedUser.login,
                    passwordHashed = dtoObj.password
                }, dtoObj.password);

                if (passVerify) //Пароль один и тот же
                    throw new Exception("password_1:1");
                else
                {
                    PasswordAppUser passwordUser = new PasswordAppUser() { login = selectedUser.login };
                    passwordUser.passwordHashed = _passwordHasher.HashPassword(passwordUser, dtoObj.password);

                    selectedUser.password = passwordUser.passwordHashed;
                    await _dbcontext.SaveChangesAsync();
                }
            }
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
