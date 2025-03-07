using ClientAPI.Interfaces;
using Middleware_Components.Broker;
using Middleware_Components.DTO.ClientAPI;
using Middleware_Components.JWT.DTO.CheckUsers;
using Middleware_Components.Services;
using ORM_Components.DTO.ClientAPI;
using ORM_Components.DTO.ClientAPI.Basket;
using ORM_Components.DTO.ClientAPI.ClientsAll;
using ORM_Components.DTO.ClientAPI.FrozenAll;
using ORM_Components.DTO.ClientAPI.OrderSelecting;
using ORM_Components.DTO.ClientAPI.RequestsAll;
using ORM_Components.DTO.PaymentAPI;
using Telegram_Components.Interfaces;

namespace ClientAPI.Services
{
    public class ClientService : IClientService
    {
        private readonly IDatabaseService _database;
        private readonly ISessionService _session;
        private readonly IJwtService _jwt;
        private readonly ICacheService _cache;
        private readonly ILogger _logger;
        private readonly IMessageSender _tgmessage;
        private readonly IRabbitMQService _rabbitMQService;

        enum StepsAuth
        {
            StepCheck = 0,
            StepUpdated
        }

        public ClientService(IRabbitMQService rabbitMQService, IMessageSender tgmessage, ISessionService session, IDatabaseService database, IJwtService jwt, ICacheService cache)
        {

            _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger("client-service-logger");
            _database = database;
            _jwt = jwt;
            _cache = cache;
            _session = session;
            _rabbitMQService = rabbitMQService;
            _tgmessage = tgmessage;
        }

        private async Task<Auth_PairTokens?> TokensReleased(Auth_CheckInfo check)
        {
            if (check.check_success == null)
                return null;

            var device_out = "неизвестного устройства";

            if (check.check_success.device == "Mobile")
                device_out = "мобильного устройства";
            else if (check.check_success.device == "PC")
                device_out = "компьютера";

            if (!_cache.CheckExistKeysStorage(check.check_success.Id, "accessTokens"))
            {
                var accessToken = _jwt.JwtTokenCreation(check.check_success);
                var refreshToken = _jwt.RefreshTokenCreation(check.check_success);

                if (_cache.CheckExistKeysStorage(check.check_success.Id, "accessTokens"))
                    _cache.DeleteKeyFromStorage(check.check_success.Id, "accessTokens");

                if (_cache.CheckExistKeysStorage(check.check_success.Id, "refreshTokens"))
                    _cache.DeleteKeyFromStorage(check.check_success.Id, "refreshTokens");

                _cache.WriteKeyInStorage(check.check_success.Id, "accessTokens", accessToken, DateTime.UtcNow.AddMinutes(10));
                _cache.WriteKeyInStorage(check.check_success.Id, "refreshTokens", refreshToken, DateTime.UtcNow.AddDays(7));

                _session.SetupSession(check.check_success.Id, accessToken);

                Auth_PairTokens pair_tokens = new Auth_PairTokens()
                {
                    accessToken = _cache.GetKeyFromStorage(check.check_success.Id, "accessTokens"),
                    refreshToken = _cache.GetKeyFromStorage(check.check_success.Id, "refreshTokens")
                };

                await _tgmessage.Send(check.check_success.telegram_chat_id.ToString(),
                    $"Техническое уведомление:\nВход с {device_out}\nAccessToken: ```{pair_tokens.accessToken}```");

                _logger.LogInformation($"Пользователь {check.check_success.Id} успешно вошел!");

                return pair_tokens;
            }
            else
            {
                var validation = await _jwt.AccessTokenValidation(
                    $"Bearer {_cache.GetKeyFromStorage(check.check_success.Id, "accessTokens")}"
                );

                if (validation.TokenHasSuccess())
                {
                    if (check.check_success.device != validation.token_success.deviceInfo)
                    {
                        var accessToken = _jwt.JwtTokenCreation(check.check_success);
                        var refreshToken = _jwt.RefreshTokenCreation(check.check_success);

                        if (_cache.CheckExistKeysStorage(check.check_success.Id, "accessTokens"))
                            _cache.DeleteKeyFromStorage(check.check_success.Id, "accessTokens");

                        if (_cache.CheckExistKeysStorage(check.check_success.Id, "refreshTokens"))
                            _cache.DeleteKeyFromStorage(check.check_success.Id, "refreshTokens");

                        _cache.WriteKeyInStorage(check.check_success.Id, "accessTokens", accessToken, DateTime.UtcNow.AddMinutes(10));
                        _cache.WriteKeyInStorage(check.check_success.Id, "refreshTokens", refreshToken, DateTime.UtcNow.AddDays(7));

                        _session.SetupSession(check.check_success.Id, accessToken);

                        Auth_PairTokens pair_tokens = new Auth_PairTokens()
                        {
                            accessToken = _cache.GetKeyFromStorage(check.check_success.Id, "accessTokens"),
                            refreshToken = _cache.GetKeyFromStorage(check.check_success.Id, "refreshTokens")
                        };

                        await _tgmessage.Send(check.check_success.telegram_chat_id.ToString(),
                            $"Техническое уведомление:\nВход с другого, а именно {device_out}\nAccessToken: ```{pair_tokens.accessToken}```");

                        _logger.LogInformation($"Пользователь {check.check_success.Id} успешно вошел!");

                        return pair_tokens;
                    }
                    else
                    {
                        Auth_PairTokens pair_tokens = new Auth_PairTokens()
                        {
                            accessToken = _cache.GetKeyFromStorage(check.check_success.Id, "accessTokens"),
                            refreshToken = _cache.GetKeyFromStorage(check.check_success.Id, "refreshTokens")
                        };

                        //await _tgmessage.Send(check.check_success.telegram_chat_id.ToString(),
                        //    $"Техническое уведомление:\nЗапрос с {device_out} без обновления токенов");

                        return pair_tokens;
                    }
                }
              
            }

            return null;
        }

        public async Task<Auth_PairTokens?> UserAuth(AuthAddUser dtoObj)
        {

            var checkUserExist = new Dictionary<StepsAuth, Auth_CheckInfo>();

            checkUserExist[StepsAuth.StepCheck] = _database.CheckUser(new AuthSignIn()
            {
                device = dtoObj.device,
                telegram_chat_id = dtoObj.chat_id
            });


            if (checkUserExist[StepsAuth.StepCheck].CheckHasSuccess())
            {

                _logger.LogWarning($"Адрес был передан: {dtoObj.address}");

                //Обновляем профиль в сервисе если есть изменения
                await _database.UserUpdateFromTelegram(
                    new ClientUpdate()
                    {
                        address = dtoObj.address,
                        first_name = dtoObj.first_name,
                        id = dtoObj.id,
                        last_name = dtoObj.last_name,
                        photo_url = dtoObj.photo_url,
                        username = dtoObj.username
                    }
                );


                checkUserExist[StepsAuth.StepUpdated] = _database.CheckUser(new AuthSignIn()
                {
                    device = dtoObj.device,
                    telegram_chat_id = dtoObj.chat_id
                });

                //Выдаем токен
                var tokens = await TokensReleased(checkUserExist[StepsAuth.StepUpdated]);

                if (tokens != null)
                    return tokens;
            }
            else
            {
                throw new InvalidOperationException("user_not_found");
            }


            return null;
        }

        public async Task<string> UserRegister(AuthAddUser dtoObj)
        {
            var checkUserExist = _database.CheckUser(new AuthSignIn()
            {
                device = dtoObj.device,
                telegram_chat_id = dtoObj.chat_id
            });

            //Если пользователя не существует в бд, создаем заявку на регу
            if (checkUserExist.CheckHasError())
            {
                if (!_cache.CheckExistKeysStorage<AuthAddUser>($"register_request_{dtoObj.id}"))
                {
                    _cache.WriteKeyInStorageObject($"register_request_{dtoObj.id}", dtoObj, DateTime.UtcNow.AddMinutes(5));

                    await _tgmessage.SendWithMarkup(dtoObj.chat_id.ToString(), "Регистрация в сервисе \"Симбир Еда\"",
                        "Подтвердить", "registerQuery");

                    return "register_request_created";
                }
                else
                {
                    return "register_request_already_exist";
                }
            }
            else
            {
                throw new InvalidOperationException("user_already_exist");
            }


        }

        public async Task<string?> ClientSignOut(string bearer_key)
        {
            var validation = await _jwt.AccessTokenValidation(bearer_key);

            if (validation.TokenHasError())
            {
                return null;
            }
            else if (validation.TokenHasSuccess())
            {
                _session.ClientSignOutSession(validation.token_success.Id);

                _cache.DeleteKeyFromStorage(validation.token_success.Id, "accessTokens");

                _cache.DeleteKeyFromStorage(validation.token_success.Id, "refreshTokens");

                _logger.LogInformation($"Пользователь id: {validation.token_success.Id} вышел!");

                //await _tgmessage.Send(validation.token_success.,
                //    $"{validation.token_success.login} | Вы вышли\nСессия завершена!");

                return $"{validation.token_success.Id}_is_logout";
            }

            return null;
        }

        public async Task<Auth_PairTokens?> RefreshClientSession(Auth_RefreshTokens dtoObj)
        {
            var validation = await _jwt.RefreshTokenValidation(dtoObj.refreshToken);

            if (validation.TokenHasError())
            {
                return null;
            }
            else if (validation.TokenHasSuccess())
            {
                Auth_CheckSuccess authsuccess = new Auth_CheckSuccess()
                {
                    Id = validation.token_success.Id,
                    roles = validation.token_success.userRoles,
                    device = validation.token_success.deviceInfo,
                };

                var accessToken = _jwt.JwtTokenCreation(authsuccess);
                var refreshToken = _jwt.RefreshTokenCreation(authsuccess);

                _session.RefreshSession(authsuccess.Id, accessToken);

                if (_cache.CheckExistKeysStorage(authsuccess.Id, "accessTokens"))
                    _cache.DeleteKeyFromStorage(authsuccess.Id, "accessTokens");

                if (_cache.CheckExistKeysStorage(authsuccess.Id, "refreshTokens"))
                    _cache.DeleteKeyFromStorage(authsuccess.Id, "refreshTokens");


                _cache.WriteKeyInStorage(authsuccess.Id, "accessTokens", accessToken, DateTime.UtcNow.AddMinutes(10));
                _cache.WriteKeyInStorage(authsuccess.Id, "refreshTokens", refreshToken, DateTime.UtcNow.AddDays(7));



                Auth_PairTokens pair_tokens = new Auth_PairTokens()
                {
                    accessToken = _cache.GetKeyFromStorage(authsuccess.Id, "accessTokens"),
                    refreshToken = _cache.GetKeyFromStorage(authsuccess.Id, "refreshTokens")
                };


                _logger.LogInformation($"Токены для id: {validation.token_success.Id} обновлены!");

                return pair_tokens;
            }

            return null;
        }


        public async Task<ClientInfo?> ClientFromIdInfo(string bearer_key, Guid userGUID)
        {
            var validation = await _jwt.AccessTokenValidation(bearer_key);

            if (validation.TokenHasError())
            {
                return null;
            }
            else if (validation.TokenHasSuccess())
            {
                if (validation.token_success.userRoles.Contains("Admin"))
                {
                    var info_user = _database.InfoClientDatabase(userGUID);

                    if (info_user != null)
                        return info_user;
                    else
                        return null;
                }
                else
                {
                    throw new Exception("role_invalid");
                }
            }

            return null;
        }

        public async Task<ClientInfo?> ClientMeInfo(string bearer_key)
        {
            var validation = await _jwt.AccessTokenValidation(bearer_key);

            if (validation.TokenHasError())
            {
                return null;
            }
            else if (validation.TokenHasSuccess())
            {
                var info_user = _database.InfoClientDatabase(validation.token_success.Id);

                if (info_user != null)
                    return info_user;
                else
                    return null;
            }

            return null;
        }

        //public async Task UpdateClientInfoWithAdmin(string bearer_key, ClientUpdate_Admin dtoObj, Guid userGUID)
        //{
        //    var validation = await _jwt.AccessTokenValidation(bearer_key);

        //    if (validation.TokenHasError())
        //    {
        //        throw new Exception("token_invalid");
        //    }
        //    else if (validation.TokenHasSuccess())
        //    {
        //        if (validation.token_success.userRoles.Contains("Admin"))
        //        {
        //            await _database.InfoClientUpdateWithAdmin(dtoObj, userGUID);
        //        }
        //        else
        //        {
        //            throw new Exception("role_invalid");
        //        }
        //    }
        //}

        public async Task<ClientGetAll?> AllProfilesGet(string bearer_key, int from, int count)
        {
            var validation = await _jwt.AccessTokenValidation(bearer_key);

            if (validation.TokenHasError())
            {
                throw new Exception("token_invalid");
            }
            else if (validation.TokenHasSuccess())
            {
                if (validation.token_success.userRoles.Contains("Admin"))
                {
                    var clientsInfo = _database.GetAllClients(from, count);

                    return clientsInfo;
                }
                else
                {
                    throw new Exception("role_invalid");
                }
            }

            return null;
        }

        //public async Task CreateClientWithAdmin(string bearer_key, ClientAdd_Admin dtoObj)
        //{
        //    var validation = await _jwt.AccessTokenValidation(bearer_key);

        //    if (validation.TokenHasError())
        //    {
        //        throw new Exception("token_invalid");
        //    }
        //    else if (validation.TokenHasSuccess())
        //    {
        //        if (validation.token_success.userRoles.Contains("Admin"))
        //        {
        //            await _database.RegisterUserWithAdmin(dtoObj);
        //        }
        //        else
        //        {
        //            throw new Exception("role_invalid");
        //        }
        //    }
        //}

        public async Task DeleteClientWithAdmin(string bearer_key, Guid userGUID)
        {
            var validation = await _jwt.AccessTokenValidation(bearer_key);

            if (validation.TokenHasError())
            {
                throw new Exception("token_invalid");
            }
            else if (validation.TokenHasSuccess())
            {
                if (validation.token_success.userRoles.Contains("Admin"))
                {
                    await _database.DeleteClientWithAdmin(userGUID);
                }
                else
                {
                    throw new Exception("role_invalid");
                }
            }
        }

        public async Task AddBasketItem(string bearer_key, Basket_Add dtoObj)
        {
            var validation = await _jwt.AccessTokenValidation(bearer_key);

            if (validation.TokenHasError())
            {
                throw new Exception("token_invalid");
            }
            else if (validation.TokenHasSuccess())
            {
                await _database.AddBasketItem(dtoObj);
            }
        }

        public async Task<Basket_GetAll?> GetItemsBasket(string bearer_key)
        {
            var validation = await _jwt.AccessTokenValidation(bearer_key);

            if (validation.TokenHasError())
            {
                throw new Exception("token_invalid");
            }
            else if (validation.TokenHasSuccess())
            {
                return await _database.GetBasketItems(validation.token_success.Id);
            }

            return null;
        }

        public async Task DeleteAllBasket(string bearer_key)
        {
            var validation = await _jwt.AccessTokenValidation(bearer_key);

            if (validation.TokenHasError())
            {
                throw new Exception("token_invalid");
            }
            else if (validation.TokenHasSuccess())
            {
                await _database.DeleteAllBasketWrites(validation.token_success.Id);
            }
        }


        public async Task DeleteOneBasketItem(string bearer_key, Guid basketId)
        {
            var validation = await _jwt.AccessTokenValidation(bearer_key);

            if (validation.TokenHasError())
            {
                throw new Exception("token_invalid");
            }
            else if (validation.TokenHasSuccess())
            {
                await _database.DeleteOneBasketWrite(validation.token_success.Id, basketId);
            }
        }

        public async Task<RequestsGetAll?> GetAllRequestsForAdmin(string bearer_key)
        {
            var validation = await _jwt.AccessTokenValidation(bearer_key);

            if (validation.TokenHasError())
            {
                throw new Exception("token_invalid");
            }
            else if (validation.TokenHasSuccess())
            {
                if (validation.token_success.userRoles.Contains("Admin"))
                {
                    var requests = _database.GetAllRequestsForAdmin();
                        
                    if (requests != null)
                    {
                        return requests;
                    }
                    else
                        throw new Exception("not_founded");
                  
                }
                else
                {
                    throw new Exception("role_invalid");
                }
            }

            return null;
        }

        public async Task CreateRestaurantRequest(string bearer_key, RestaurantAddRequest dtoObj)
        {
            var validation = await _jwt.AccessTokenValidation(bearer_key);

            if (validation.TokenHasError())
            {
                throw new Exception("token_invalid");
            }
            else if (validation.TokenHasSuccess())
            {
                var chatId = _database.GetTelegramChatId(validation.token_success.Id);

                var requestId = await _database.CreateRequestRestaurantFromUser(validation.token_success.Id, dtoObj);

                await _tgmessage.Send(chatId, $"Ваша заявка на создание ресторана, была создана!\nНомер заявки: {requestId}");
            }
        }

        public async Task CreateCourierRequest(string bearer_key, string? car_number, string description)
        {
            var validation = await _jwt.AccessTokenValidation(bearer_key);

            if (validation.TokenHasError())
            {
                throw new Exception("token_invalid");
            }
            else if (validation.TokenHasSuccess())
            {
                var chatId = _database.GetTelegramChatId(validation.token_success.Id);

                var requestId = await _database.CreateRequestCourierFromUser(validation.token_success.Id, car_number, description);

                await _tgmessage.Send(chatId, $"Ваша заявка на работу курьером, была создана!\nНомер заявки: {requestId}");
            }
        }

        public async Task<List<RequestInfo_Restaurants>?> GetMeRequestsRestaurant(string bearer_key)
        {
            var validation = await _jwt.AccessTokenValidation(bearer_key);

            if (validation.TokenHasError())
            {
                throw new Exception("token_invalid");
            }
            else if (validation.TokenHasSuccess())
            {
                var requests = _database.GetOnlyMeRequestsRestaurant(validation.token_success.Id);

                if (requests != null)
                {
                    return requests;
                }
            }

            return null;
        }

        public async Task<RequestInfo_Couriers?> GetMeRequestCourier(string bearer_key)
        {
            var validation = await _jwt.AccessTokenValidation(bearer_key);

            if (validation.TokenHasError())
            {
                throw new Exception("token_invalid");
            }
            else if (validation.TokenHasSuccess())
            {
                var request = _database.GetOnlyMeRequestCourier(validation.token_success.Id);

                if (request != null)
                {
                    return request;
                }
            }

            return null;
        }

        public async Task AcceptRequests(string bearer_key, Guid requestId, string type)
        {
            var validation = await _jwt.AccessTokenValidation(bearer_key);

            if (validation.TokenHasError())
            {
                throw new Exception("token_invalid");
            }
            else if (validation.TokenHasSuccess())
            {
                if (validation.token_success.userRoles.Contains("Admin"))
                {
                    var chatId = _database.GetTelegramChatIdFromRequestId(requestId);

                    if (type == "restaurant")
                    {
                        await _database.AcceptRequestRestaurantFromAdmin(requestId);

                        await _tgmessage.Send(chatId, $"Ваша заявка по созданию ресторана, была одобрена!\nНомер заявки: {requestId}");
                    }
                    else if (type == "courier")
                    {
                        await _database.AcceptRequestCourierFromAdmin(requestId);

                        await _tgmessage.Send(chatId, $"Ваша заявка на работу курьером, была одобрена!\nНомер заявки: {requestId}");
                    }
                }
                else
                {
                    throw new Exception("role_invalid");
                }
            }
        }

        public async Task RejectRequests(string bearer_key, Guid requestId, string type)
        {
            var validation = await _jwt.AccessTokenValidation(bearer_key);

            if (validation.TokenHasError())
            {
                throw new Exception("token_invalid");
            }
            else if (validation.TokenHasSuccess())
            {
                if (validation.token_success.userRoles.Contains("Admin"))
                {
                    var chatId = _database.GetTelegramChatIdFromRequestId(requestId);

                    if (type == "restaurant")
                    {
                        await _database.RejectRequestRestaurantFromAdmin(requestId);

                        await _tgmessage.SendWithMarkup(chatId, $"Ваша заявка по созданию ресторана, была отклонена!\nНомер заявки: {requestId}",
                            "Понятно", "AcceptButtonQuery");
                    }
                    else if (type == "courier")
                    {
                        await _database.RejectRequestCourierFromAdmin(requestId);

                        await _tgmessage.SendWithMarkup(chatId, $"Ваша заявка на работу курьером, была отклонена!\nНомер заявки: {requestId}",
                            "Понятно", "AcceptButtonQuery");
                    }
                }
                else
                {
                    throw new Exception("role_invalid");
                }
            }
        }

        public async Task FreezeWorkRestaurantWithAdmin(string bearer_key, Guid restaurantId, Downgrade dtoObj)
        {
            var validation = await _jwt.AccessTokenValidation(bearer_key);

            if (validation.TokenHasError())
            {
                throw new Exception("token_invalid");
            }
            else if (validation.TokenHasSuccess())
            {
                if (validation.token_success.userRoles.Contains("Admin"))
                {
                    var userId = await _database.FreezeRestaurantWork(restaurantId);

                    var chatId = _database.GetTelegramChatId(userId);

                    await _tgmessage.Send(chatId, $"Работа вашего ресторана ({restaurantId}) приостановлена\nПричина: {dtoObj.reason}");
                }
                else
                {
                    throw new Exception("role_invalid");
                }
            }
        }

        public async Task FreezeWorkCourierWithAdmin(string bearer_key, Guid userGUID, Downgrade dtoObj)
        {
            var validation = await _jwt.AccessTokenValidation(bearer_key);

            if (validation.TokenHasError())
            {
                throw new Exception("token_invalid");
            }
            else if (validation.TokenHasSuccess())
            {
                if (validation.token_success.userRoles.Contains("Admin"))
                {
                    await _database.FreezeCourierWork(userGUID);

                    var chatId = _database.GetTelegramChatId(userGUID);

                    await _tgmessage.Send(chatId, $"Ваша работа курьером приостановлена\nПричина: {dtoObj.reason}");
                }
                else
                {
                    throw new Exception("role_invalid");
                }
            }
        }

        public async Task UnfreezeRestaurantWithAdmin(string bearer_key, Guid restaurantId)
        {
            var validation = await _jwt.AccessTokenValidation(bearer_key);

            if (validation.TokenHasError())
            {
                throw new Exception("token_invalid");
            }
            else if (validation.TokenHasSuccess())
            {
                if (validation.token_success.userRoles.Contains("Admin"))
                {
                    var userId = await _database.UnfreezeRestaurantWork(restaurantId);

                    var chatId = _database.GetTelegramChatId(userId);

                    await _tgmessage.Send(chatId, $"Работа вашего ресторана ({restaurantId}) возобновлена\nРады снова сотрудничать!");
                }
                else
                {
                    throw new Exception("role_invalid");
                }
            }
        }

        public async Task UnfreezeCourierWithAdmin(string bearer_key, Guid userGUID)
        {
            var validation = await _jwt.AccessTokenValidation(bearer_key);

            if (validation.TokenHasError())
            {
                throw new Exception("token_invalid");
            }
            else if (validation.TokenHasSuccess())
            {
                if (validation.token_success.userRoles.Contains("Admin"))
                {
                    await _database.UnfreezeCourierWork(userGUID);

                    var chatId = _database.GetTelegramChatId(userGUID);

                    await _tgmessage.Send(chatId, $"Вы снова можете продолжить работать курьером\nРады снова сотрудничать!");
                }
                else
                {
                    throw new Exception("role_invalid");
                }
            }
        }

        public async Task<FrozenGetAll?> GetAllFrozenEntities(string bearer_key)
        {
            var validation = await _jwt.AccessTokenValidation(bearer_key);

            if (validation.TokenHasError())
            {
                throw new Exception("token_invalid");
            }
            else if (validation.TokenHasSuccess())
            {
                if (validation.token_success.userRoles.Contains("Admin"))
                {
                    return _database.GetAllFrozenEntities();
                }
                else
                {
                    throw new Exception("role_invalid");
                }
            }

            return null;
        }

        public async Task CreateOrder(string bearer_key)
        {
            var validation = await _jwt.AccessTokenValidation(bearer_key);

            if (validation.TokenHasError())
            {
                throw new Exception("token_invalid");
            }
            else if (validation.TokenHasSuccess())
            {
                var order = await _database.CreateOrder(validation.token_success.Id);

                var moneyValue = _database.GetUserBalance(validation.token_success.Id);

                var chatId = _database.GetTelegramChatId(validation.token_success.Id);

                //Отправка в ресторан
                _rabbitMQService.SendMessage<Order_DTO>("client_to_restaurant", order);

                //Сообщение пользователю
                await _tgmessage.SendHtml(chatId, $"Заказ {order.id} оплачен\nОстаток баланса: <tg-spoiler>{moneyValue}</tg-spoiler> руб");
            }
        }

        public async Task<OrderInfo?> GetOrderFromId(string bearer_key, Guid orderId)
        {
            var validation = await _jwt.AccessTokenValidation(bearer_key);

            if (validation.TokenHasError())
            {
                throw new Exception("token_invalid");
            }
            else if (validation.TokenHasSuccess())
            {
                var order = _database.GetOrderInfoFromId(orderId);

                return order;
            }

            return null;
        }

        public async Task<List<OrderInfo>?> GetAllOrders(string bearer_key)
        {
            var validation = await _jwt.AccessTokenValidation(bearer_key);

            if (validation.TokenHasError())
            {
                throw new Exception("token_invalid");
            }
            else if (validation.TokenHasSuccess())
            {
                var order = _database.GetAllOrders(validation.token_success.Id);

                return order;
            }

            return null;
        }

        public async Task<List<OrderInfo_History>?> GetAllHistoryOrder(string bearer_key, Guid orderId)
        {
            var validation = await _jwt.AccessTokenValidation(bearer_key);

            if (validation.TokenHasError())
            {
                throw new Exception("token_invalid");
            }
            else if (validation.TokenHasSuccess())
            {
                if (validation.token_success.userRoles.Contains("Admin"))
                {
                    var order = _database.GetHistoryStatusOrder(orderId);

                    return order;
                }
                else
                {
                    throw new Exception("role_invalid");
                }

             
            }

            return null;
        }

        public async Task MoneyOut(string bearer_key, PaymentOut dtoObj)
        {
            var validation = await _jwt.AccessTokenValidation(bearer_key);

            if (validation.TokenHasError())
            {
                throw new Exception("token_invalid");
            }
            else if (validation.TokenHasSuccess())
            {
                _logger.LogWarning($"Было запрошено ({dtoObj.money_value} руб)");

                if (_database.ExistMoney(validation.token_success.Id, dtoObj.money_value))
                {
                    var chatId = _database.GetTelegramChatId(validation.token_success.Id);

                    //await _database.DecreaseMoney(validation.token_success.Id, dtoObj.money_value);

                    var moneyValue = _database.GetUserBalance(validation.token_success.Id);

                    await _tgmessage.SendHtml(chatId, $"Запрос на вывод с баланса: <tg-spoiler>{dtoObj.money_value}</tg-spoiler> руб");

                    _rabbitMQService.SendMessage("client_to_payment", new Payment_Out_Queue()
                    {
                        card_number = dtoObj.card_number,
                        money_value = dtoObj.money_value,
                        user_id = validation.token_success.Id
                    });

                    _logger.LogWarning($"Было отправлено ({dtoObj.money_value} руб)");

                }
            }
        }

        public async Task ChangeOrAddEmail(string bearer_key, string email)
        {
            var validation = await _jwt.AccessTokenValidation(bearer_key);

            if (validation.TokenHasError())
            {
                throw new Exception("token_invalid");
            }
            else if (validation.TokenHasSuccess())
            {
                await _database.ChangeOrAddEmail(email, validation.token_success.Id);
            }
        }
    }
}
