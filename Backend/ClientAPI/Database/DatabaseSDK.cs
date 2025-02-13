using ClientAPI.Services;
using ORM_Components.DTO.ClientAPI;
using ORM_Components.Tables;
using Microsoft.AspNetCore.Identity;
using ORM_Components;
using ORM_Components.DTO.CheckUsers;

namespace ClientAPI.Database
{
    public class DatabaseSDK : IDatabaseService
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _conf;
        private readonly DataContext _dbcontext;
        private readonly PasswordHasher<PasswordAppUser> _passwordHasher;

        public DatabaseSDK(IConfiguration configuration, DataContext dbcontext)
        {
            _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger("ClientAPI | database-sdk-logger");
            _conf = configuration;
            _passwordHasher = new PasswordHasher<PasswordAppUser>();
            _dbcontext = dbcontext;
        }

        public async Task RegisterUser(AuthSignUp dto)
        {
            if (dto == null)
            {
                _logger.LogError("RegisterUser: dto==null");
                return;
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
                roles = new string[] { "Client" }
            };

            _dbcontext.userTable.Add(usersTable);
            await _dbcontext.SaveChangesAsync();

            _logger.LogInformation($"RegisterUser: {dto.login}, создан");
            
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
                c => (c.login == dto.login)
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
                            Id = userFound.id,
                            username = userFound.login,
                            roles = userFound.roles.ToList()
                        }
                    };
            }
        

            _logger.LogError("CheckUser: Пользователь ввел неверно имя или пароль!");
            return new Auth_CheckInfo() { check_error = new Auth_CheckError { errorLog = "username/password_incorrect" } };
        }

    }
}
