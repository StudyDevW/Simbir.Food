using ORM_Components.DTO.ClientAPI;
using ORM_Components.Tables;
using Microsoft.AspNetCore.Identity;
using ORM_Components;
using ClientAPI.Interfaces;
using Middleware_Components.JWT.DTO.CheckUsers;
using ORM_Components.DTO.ClientAPI.ClientsAll;
using Microsoft.EntityFrameworkCore;
using ORM_Components.DTO.ClientAPI.Basket;
using Telegram.Bot.Types;
using ORM_Components.Tables.Helpers;
using ORM_Components.DTO.ClientAPI.RequestsAll;
using ORM_Components.DTO.ClientAPI.FrozenAll;
using ORM_Components.DTO.ClientAPI.OrderSelecting;
using Microsoft.Extensions.Logging;
using ORM_Components.DTO.ClientAPI.Review;
using System.Linq;

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

            if (dtoObj.last_name != "" && dtoObj.last_name != null && selectedUser.last_name != dtoObj.last_name)
            {
                selectedUser.last_name = dtoObj.last_name;
                await _dbcontext.SaveChangesAsync();
            }

            if (dtoObj.address != "" && dtoObj.address != "NO_CHANGE" && selectedUser.address != dtoObj.address)
            {
                selectedUser.address = dtoObj.address;
                await _dbcontext.SaveChangesAsync();
            }

            if (selectedUser.username != dtoObj.username && dtoObj.username != null)
            {
                selectedUser.username = dtoObj.username;
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

        //Проверка на то владеет ли пользователь ресторанами или нет
        private List<Guid>? RestaurantOwner(Guid userGUID)
        {
            var selectedRestaurants = _dbcontext.restaurantTable.Where(c => c.user_id == userGUID).ToList();

            if (selectedRestaurants != null)
            {
                var outputVals = new List<Guid>();

                foreach (var restaurant in selectedRestaurants)
                {
                    outputVals.Add(restaurant.Id);
                }

                return outputVals;
            }

            return null;
        }

        public ClientInfo? InfoClientDatabase(Guid userGUID)
        {
            var selectedUser = _dbcontext.userTable.Where(c => c.Id == userGUID).FirstOrDefault();

            if (selectedUser != null)
            {
                _logger.LogInformation($"InfoClientDatabase: Запрошена информация о аккаунте (id: {userGUID})");

                _logger.LogWarning($"InfoClientDatabase: информация о аккаунте (address: {selectedUser.address})");

                var restaurantOwnerId = RestaurantOwner(userGUID);

                var basketItems = _dbcontext.basketTable.Where(c => c.user_id == userGUID).ToList();

                var orderItems = _dbcontext.orderTable.Where(c => c.client_id == userGUID).ToList();

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
                    basket_items = basketItems.Count,
                    orders_count = orderItems.Count,
                    restaurant_own = restaurantOwnerId,
                    money_value = selectedUser.money_value,
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
                var filteredQuery = _dbcontext.userTable.Skip(_from).Take(_count).ToList();

                foreach (var client in filteredQuery)
                {
                    var restaurantOwnerId = RestaurantOwner(client.Id);

                    var basketItems = _dbcontext.basketTable.Where(c => c.user_id == client.Id).ToList();

                    var orderItems = _dbcontext.orderTable.Where(c => c.client_id == client.Id).ToList();

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
                        restaurant_own = restaurantOwnerId,
                        basket_items = basketItems.Count,
                        orders_count = orderItems.Count,
                        money_value = client.money_value,
                        roles = client.roles.ToList()
                    };

                    clients.Add(clientInfo);
                }
            }
            else
            {
                var filteredQuery = _dbcontext.userTable.Skip(_from).ToList();

                foreach (var client in filteredQuery)
                {
                    var restaurantOwnerId = RestaurantOwner(client.Id);

                    var basketItems = _dbcontext.basketTable.Where(c => c.user_id == client.Id).ToList();

                    var orderItems = _dbcontext.orderTable.Where(c => c.client_id == client.Id).ToList();

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
                        restaurant_own = restaurantOwnerId,
                        basket_items = basketItems.Count,
                        orders_count = orderItems.Count,
                        money_value = client.money_value,
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

        private async Task DeleteFullBasket(Guid userGUID)
        {
            var selectedItems = _dbcontext.basketTable.Where(c => c.user_id == userGUID).ToList();

            foreach (var foodItem in selectedItems)
            {
                _dbcontext.basketTable.Remove(foodItem);
                await _dbcontext.SaveChangesAsync();
            }
        }

        private async Task CheckExistItemBasket(Guid userGUID, Guid foodItemId)
        {
            var selectedBasketItems = _dbcontext.basketTable.Where(c => c.user_id == userGUID).ToList();

            var selectedNewItem = _dbcontext.restaurantFoodItemsTable
                    .Where(c => c.Id == foodItemId)
                    .FirstOrDefault();


            foreach (var basketItem in selectedBasketItems)
            {
                var selectedExistsItem = _dbcontext.restaurantFoodItemsTable
                    .Where(c => c.Id == basketItem.food_item_id)
                    .FirstOrDefault();

                //Рестораны не совпадают, удаляем корзину
                if (selectedNewItem?.restaurant_id != selectedExistsItem?.restaurant_id)
                {
                    await DeleteFullBasket(userGUID);
                }
            }
        }

        public async Task AddBasketItem(Guid fooditemId, Guid userGUID)
        {
            var selectedUser = _dbcontext.userTable.Where(c => c.Id == userGUID).FirstOrDefault();

            if (selectedUser == null)
                throw new Exception("user_not_found");

            var selectedFoodItem = _dbcontext.restaurantFoodItemsTable.Where(c => c.Id == fooditemId).FirstOrDefault();

            if (selectedFoodItem == null)
                throw new Exception("food_item_not_found");

            await CheckExistItemBasket(userGUID, fooditemId);

            BasketTable basket = new BasketTable()
            {
                user_id = userGUID,
                food_item_id = fooditemId,
            };

            _dbcontext.basketTable.Add(basket);
            await _dbcontext.SaveChangesAsync();

            _logger.LogInformation($"AddBasketItem: Товар {fooditemId} был добавлен в корзину пользователя {userGUID}");

        }

        public async Task<Basket_GetAll> GetBasketItems(Guid userGUID)
        {
            Basket_GetAll basket_GetAll = new Basket_GetAll()
            {
                basketItem = new List<Basket_GetAll_Item>(),
                basketInfo = new Basket_GetAll_Info()
            };

            List<Basket_GetAll_Item> basketItems = new List<Basket_GetAll_Item>();

            Basket_GetAll_Info basketInfo = new Basket_GetAll_Info();

            long priceFinal = 0;
            int countFinal = 0;

            var basketItemDbObj = _dbcontext.basketTable.Where(c => c.user_id == userGUID).ToListAsync();

            foreach (var basketItemDb in await basketItemDbObj)
            {
                var selectedFoodItem = _dbcontext.restaurantFoodItemsTable.Where(c => c.Id == basketItemDb.food_item_id).FirstOrDefault();

                if (selectedFoodItem != null)
                {
                    Basket_GetAll_Item basketItem = new Basket_GetAll_Item()
                    {
                        id = basketItemDb.Id,
                        restaurant_id = selectedFoodItem.restaurant_id,
                        name = selectedFoodItem.name,
                        weight = selectedFoodItem.weight,
                        calories = selectedFoodItem.calories,
                        image = selectedFoodItem.image,
                        price = selectedFoodItem.price
                    };

                    priceFinal += basketItem.price;
                    countFinal++;
                    basketItems.Add(basketItem);
                }
            }


            basketInfo = new Basket_GetAll_Info()
            {
                count = countFinal,
                totalPrice = priceFinal
            };

            basket_GetAll = new Basket_GetAll()
            {
                basketItem = basketItems,
                basketInfo = basketInfo
            };

            return basket_GetAll;
        }

        public async Task DeleteAllBasketWrites(Guid userGUID)
        {
            await DeleteFullBasket(userGUID);
        }

        public async Task DeleteOneBasketWrite(Guid userGUID, Guid basketId)
        {
            var selectedItem = _dbcontext.basketTable.Where(c => c.Id == basketId && c.user_id == userGUID).FirstOrDefault();

            if (selectedItem != null)
            {
                _dbcontext.basketTable.Remove(selectedItem);
                await _dbcontext.SaveChangesAsync();
            }
            else
                throw new Exception("write_not_founded");
        }

        public async Task<Guid> CreateRequestRestaurantFromUser(Guid userGUID, RestaurantAddRequest dtoObj)
        {
            var selectedRestaurants = _dbcontext.restaurantTable.Where(c => c.user_id == userGUID).ToList();

            if (selectedRestaurants.Count > 5)
                throw new Exception("max_restaurants_detected");

            RestaurantTable restaurantAdd = new RestaurantTable()
            {
                user_id = userGUID,
                restaurantName = dtoObj.restaurantName,
                address = dtoObj.address,
                phone_number = dtoObj.phone_number,
                status = RestaurantStatus.Unverified,
                description = dtoObj.description,
                imagePath = dtoObj.imagePath,
                open_time = dtoObj.open_time,
                close_time = dtoObj.close_time
            };

            _dbcontext.restaurantTable.Add(restaurantAdd);
            await _dbcontext.SaveChangesAsync();

            RequestTable requestAdd = new RequestTable()
            {
                restaurant_id = restaurantAdd.Id,
                courier_id = null,
                user_id = userGUID,
                time_add = DateTime.UtcNow,
                description = dtoObj.request_description
            };

            _dbcontext.requestTable.Add(requestAdd);
            await _dbcontext.SaveChangesAsync();

            return requestAdd.Id;
        }

        public async Task<Guid> CreateRequestCourierFromUser(Guid userGUID, string? car_number, string description)
        {
            var selectedCouriers = _dbcontext.courierTable.Where(c => c.userId == userGUID).ToList();

            if (selectedCouriers.Count > 0)
                throw new Exception("max_courier_detected");

            CourierTable courierAdd = new CourierTable() { 
                userId = userGUID,
                car_number = car_number,
                status = CourierStatus.Unverified
            };

            _dbcontext.courierTable.Add(courierAdd);
            await _dbcontext.SaveChangesAsync();

            RequestTable requestAdd = new RequestTable()
            {
                restaurant_id = null,
                courier_id = courierAdd.Id,
                user_id = userGUID,
                time_add = DateTime.UtcNow,
                description = description
            };

            _dbcontext.requestTable.Add(requestAdd);
            await _dbcontext.SaveChangesAsync();

            return requestAdd.Id;
        }

        public async Task AcceptRequestRestaurantFromAdmin(Guid requestId)
        {
            var selectedRequest = _dbcontext.requestTable.Where(c => c.Id == requestId).FirstOrDefault();

  
            if (selectedRequest != null/* && selectedRestaurant != null*/)
            {
                var selectedRestaurant = _dbcontext.restaurantTable.Where(c => c.user_id == selectedRequest.user_id && c.Id == selectedRequest.restaurant_id).FirstOrDefault();

                if (selectedRestaurant == null)
                    throw new Exception("restaurant_not_found");

                selectedRestaurant.status = RestaurantStatus.Verified;
                await _dbcontext.SaveChangesAsync();

                _dbcontext.requestTable.Remove(selectedRequest);
                await _dbcontext.SaveChangesAsync();
            }
            else
            {
                throw new Exception("request_not_found");
            }
        }

        public async Task AcceptRequestCourierFromAdmin(Guid requestId)
        {
            var selectedRequest = _dbcontext.requestTable.Where(c => c.Id == requestId).FirstOrDefault();

            if (selectedRequest != null/* && selectedCourier != null*/)
            {
                var selectedCourier = _dbcontext.courierTable.Where(c => c.userId == selectedRequest.user_id && c.Id == selectedRequest.courier_id).FirstOrDefault();

                if (selectedCourier == null)
                    throw new Exception("courier_not_found");

                var selectedUser = _dbcontext.userTable.Where(c => c.Id == selectedCourier.userId).FirstOrDefault();

                if (selectedUser == null)
                    throw new Exception("user_not_found");

                { //Присваивание новой роли
                    var existRoles = selectedUser.roles.ToList();

                    existRoles.Add("Courier");

                    selectedUser.roles = existRoles.ToArray();
                    await _dbcontext.SaveChangesAsync();
                }


                selectedCourier.status = CourierStatus.IsInactive;
                await _dbcontext.SaveChangesAsync();

                _dbcontext.requestTable.Remove(selectedRequest);
                await _dbcontext.SaveChangesAsync();
            }
            else
            {
                throw new Exception("request_not_found");
            }
        }

        public async Task RejectRequestRestaurantFromAdmin(Guid requestId)
        {
            var selectedRequest = _dbcontext.requestTable.Where(c => c.Id == requestId).FirstOrDefault();
          
            if (selectedRequest != null)
            {
                var selectedRestaurant = _dbcontext.restaurantTable.Where(c => c.user_id == selectedRequest.user_id && c.Id == selectedRequest.restaurant_id).FirstOrDefault();

                if (selectedRestaurant == null)
                    throw new Exception("restaurant_not_found");

                _dbcontext.restaurantTable.Remove(selectedRestaurant);
                await _dbcontext.SaveChangesAsync();

                _dbcontext.requestTable.Remove(selectedRequest);
                await _dbcontext.SaveChangesAsync();
            }
            else
            {
                throw new Exception("request_not_found");
            }
        }

        public async Task RejectRequestCourierFromAdmin(Guid requestId)
        {
            var selectedRequest = _dbcontext.requestTable.Where(c => c.Id == requestId).FirstOrDefault();

            if (selectedRequest != null)
            {
                var selectedCourier = _dbcontext.courierTable.Where(c => c.userId == selectedRequest.user_id && c.Id == selectedRequest.courier_id).FirstOrDefault();

                if (selectedCourier == null)
                    throw new Exception("courier_not_found");

                _dbcontext.courierTable.Remove(selectedCourier);
                await _dbcontext.SaveChangesAsync();

                _dbcontext.requestTable.Remove(selectedRequest);
                await _dbcontext.SaveChangesAsync();
            }
            else
            {
                throw new Exception("request_not_found");
            }
        }

        public List<RequestInfo_Restaurants> GetOnlyMeRequestsRestaurant(Guid user_id)
        {
            List<RequestInfo_Restaurants> listRequestsRestaurants = new List<RequestInfo_Restaurants>();

            var selectedRequests = _dbcontext.requestTable.Where(c => c.user_id == user_id).ToList();

            if (selectedRequests != null)
            {
                foreach (var request in selectedRequests)
                {
                    var selectedRestaurants = _dbcontext.restaurantTable.Where(c => c.Id == request.restaurant_id).ToList();

                    foreach (var restaurant in selectedRestaurants)
                    {
                        var selectedUser = _dbcontext.userTable.Where(c => c.Id == user_id).FirstOrDefault();

                        if (selectedUser == null)
                            throw new Exception("user_not_found");

                        RequestInfo_Restaurants requestInfo_Restaurants = new RequestInfo_Restaurants()
                        {
                            request_id = request.Id,
                            restaurantName = restaurant.restaurantName,
                            address = restaurant.address,
                            phone_number = restaurant.phone_number,
                            description = restaurant.description,
                            imagePath = restaurant.imagePath,
                            open_time = restaurant.open_time,
                            close_time = restaurant.close_time,
                            request_description = request.description,
                            request_time_add = request.time_add,
                            client_info = new RequestClientInfo()
                            {
                                Id = selectedUser.Id,
                                first_name = selectedUser.first_name,
                                last_name = selectedUser.last_name,
                                address = selectedUser.address,
                                chat_id = selectedUser.telegram_chat_id,
                                username = selectedUser.username,
                                photo_url = selectedUser.photo_url,
                                roles = selectedUser.roles.ToList()
                            }
                        };

                        listRequestsRestaurants.Add(requestInfo_Restaurants);
                    }
                }
            }

            return listRequestsRestaurants;
        }

        public RequestInfo_Couriers? GetOnlyMeRequestCourier(Guid user_id)
        {
   
            var selectedRequests = _dbcontext.requestTable.Where(c => c.user_id == user_id).ToList();

            if (selectedRequests != null)
            {
                foreach (var request in selectedRequests)
                {
                    var selectedCourier = _dbcontext.courierTable.Where(c => c.Id == request.courier_id).FirstOrDefault();

                    if (selectedCourier != null)
                    {
                        var selectedUser = _dbcontext.userTable.Where(c => c.Id == selectedCourier.userId).FirstOrDefault();

                        if (selectedUser == null)
                            throw new Exception("user_not_found");

                        RequestInfo_Couriers requestInfo_Courier = new RequestInfo_Couriers()
                        {
                            request_id = request.Id,
                            car_number = selectedCourier.car_number,
                            request_description = request.description,
                            request_time_add = request.time_add,
                            client_info = new RequestClientInfo()
                            {
                                Id = selectedUser.Id,
                                first_name = selectedUser.first_name,
                                last_name = selectedUser.last_name,
                                address = selectedUser.address,
                                chat_id = selectedUser.telegram_chat_id,
                                username = selectedUser.username,
                                photo_url = selectedUser.photo_url,
                                roles = selectedUser.roles.ToList()
                            }
                        };

                        return requestInfo_Courier;
                    }
                }
            }

            return null;
        }

        public RequestsGetAll GetAllRequestsForAdmin()
        {
            RequestsGetAll requestsAll = new RequestsGetAll();

            List<RequestInfo_Restaurants> listRequestsRestaurants = new List<RequestInfo_Restaurants>();
            
            List<RequestInfo_Couriers> listRequestsCouriers = new List<RequestInfo_Couriers>();

            var selectedRequests = _dbcontext.requestTable.ToList();

            if (selectedRequests != null)
            {
                foreach (var request in selectedRequests)
                {
                    var selectedRestaurants = _dbcontext.restaurantTable.Where(c => c.Id == request.restaurant_id).ToList();

                    foreach (var restaurant in selectedRestaurants)
                    {
                        var selectedUser = _dbcontext.userTable.Where(c => c.Id == restaurant.user_id).FirstOrDefault();

                        if (selectedUser == null)
                            throw new Exception("user_not_found");

                        RequestInfo_Restaurants requestInfo_Restaurants = new RequestInfo_Restaurants()
                        {
                            request_id = request.Id,
                            restaurantName = restaurant.restaurantName,
                            address = restaurant.address,
                            phone_number = restaurant.phone_number,
                            description = restaurant.description,
                            imagePath = restaurant.imagePath,
                            open_time = restaurant.open_time,
                            close_time = restaurant.close_time,
                            request_description = request.description,
                            request_time_add = request.time_add,
                            client_info = new RequestClientInfo()
                            {
                                Id = selectedUser.Id,
                                first_name = selectedUser.first_name,
                                last_name = selectedUser.last_name,
                                address = selectedUser.address,
                                chat_id = selectedUser.telegram_chat_id,
                                username = selectedUser.username,   
                                photo_url = selectedUser.photo_url,
                                roles = selectedUser.roles.ToList()
                            }
                        };

                        listRequestsRestaurants.Add(requestInfo_Restaurants);
                    }


                    var selectedCouriers = _dbcontext.courierTable.Where(c => c.Id == request.courier_id).ToList();

                    foreach (var courier in selectedCouriers)
                    {
                        var selectedUser = _dbcontext.userTable.Where(c => c.Id == courier.userId).FirstOrDefault();

                        if (selectedUser == null)
                            throw new Exception("user_not_found");

                        RequestInfo_Couriers requestInfo_Couriers = new RequestInfo_Couriers()
                        {
                            request_id = request.Id,
                            car_number = courier.car_number,
                            request_description = request.description,
                            request_time_add = request.time_add,
                            client_info = new RequestClientInfo() {
                                Id = selectedUser.Id,
                                first_name = selectedUser.first_name,
                                last_name = selectedUser.last_name,
                                address = selectedUser.address,
                                chat_id = selectedUser.telegram_chat_id,
                                username = selectedUser.username,
                                photo_url = selectedUser.photo_url,
                                roles = selectedUser.roles.ToList()
                            }
                        };

                        listRequestsCouriers.Add(requestInfo_Couriers);
                    }
                }
            }

            requestsAll.RestaurantFill(listRequestsRestaurants);

            requestsAll.CourierFill(listRequestsCouriers);

            return requestsAll;
        }


        public FrozenGetAll GetAllFrozenEntities()
        {
            FrozenGetAll frozenAll = new FrozenGetAll();

            List<FrozenInfo_Restaurants> listFrozenRestaurants = new List<FrozenInfo_Restaurants>();

            List<FrozenInfo_Couriers> listFrozenCouriers = new List<FrozenInfo_Couriers>();

            var selectedRestaurants = _dbcontext.restaurantTable.Where(c => c.status == RestaurantStatus.Frozen).ToList();

            foreach (var restaurant in selectedRestaurants)
            {
                FrozenInfo_Restaurants frozenInfo_Restaurant = new FrozenInfo_Restaurants()
                {
                    restaurantId = restaurant.Id,
                    address = restaurant.address,
                    imagePath = restaurant.imagePath,
                    restaurantName = restaurant.restaurantName,
                    user_id = restaurant.user_id
                };

                listFrozenRestaurants.Add(frozenInfo_Restaurant);
            }

            var selectedCouriers = _dbcontext.courierTable.Where(c => c.status == CourierStatus.Frozen).ToList();

            foreach (var courier in selectedCouriers)
            {
                var selectedUser = _dbcontext.userTable.Where(c => c.Id == courier.userId).FirstOrDefault();

                if (selectedUser == null)
                    throw new Exception("user_not_found");

                FrozenInfo_Couriers frozenInfo_Courier = new FrozenInfo_Couriers()
                {
                    first_name = selectedUser.first_name,
                    last_name = selectedUser.last_name,
                    user_id = selectedUser.Id,
                    photo_url = selectedUser.photo_url
                };

                listFrozenCouriers.Add(frozenInfo_Courier);
            }

            frozenAll.RestaurantFill(listFrozenRestaurants);

            frozenAll.CourierFill(listFrozenCouriers);

            return frozenAll;
        }

        public async Task<Guid> UnfreezeRestaurantWork(Guid restaurantId)
        {
            var selectedRestaurant = _dbcontext.restaurantTable.Where(c => c.Id == restaurantId).FirstOrDefault();

            if (selectedRestaurant == null)
            {
                throw new Exception("restaurant_not_found");
            }

            if (selectedRestaurant.status == RestaurantStatus.Verified)
            {
                throw new Exception("restaurant_not_frozen");
            }

            if (selectedRestaurant.status != RestaurantStatus.Unverified)
            {
                selectedRestaurant.status = RestaurantStatus.Verified;
                await _dbcontext.SaveChangesAsync();
                return selectedRestaurant.user_id;
            }
            else
            {
                throw new Exception("restaurant_unverified_now");
            }
        }

        public async Task UnfreezeCourierWork(Guid userGUID)
        {
            var selectedCourier = _dbcontext.courierTable.Where(c => c.userId == userGUID).FirstOrDefault();

            if (selectedCourier == null)
            {
                throw new Exception("courier_not_found");
            }

            if (selectedCourier.status == CourierStatus.IsInactive || selectedCourier.status == CourierStatus.IsActive)
            {
                throw new Exception("courier_not_frozen");
            }

            if (selectedCourier.status != CourierStatus.Unverified)
            {
                selectedCourier.status = CourierStatus.IsInactive;
                await _dbcontext.SaveChangesAsync();
            }
            else
            {
                throw new Exception("courier_unverified_now");
            }
        }

        public async Task<Guid> FreezeRestaurantWork(Guid restaurantId)
        {
            var selectedRestaurant = _dbcontext.restaurantTable.Where(c => c.Id == restaurantId).FirstOrDefault();
     
            if (selectedRestaurant == null) {
                throw new Exception("restaurant_not_found");
            }

            if (selectedRestaurant.status == RestaurantStatus.Frozen)
                throw new Exception("restaurant_already_frozen");

            if (selectedRestaurant.status != RestaurantStatus.Unverified)
            {
                selectedRestaurant.status = RestaurantStatus.Frozen;
                await _dbcontext.SaveChangesAsync();
                return selectedRestaurant.user_id;
            }
            else
            {
                throw new Exception("restaurant_unverified_now");
            }
        }

        public async Task FreezeCourierWork(Guid userGUID)
        {
            var selectedCourier = _dbcontext.courierTable.Where(c => c.userId == userGUID).FirstOrDefault();

            if (selectedCourier == null)
            {
                throw new Exception("courier_not_found");
            }

            if (selectedCourier.status == CourierStatus.Frozen)
                throw new Exception("courier_already_frozen");

            if (selectedCourier.status != CourierStatus.Unverified)
            {
                selectedCourier.status = CourierStatus.Frozen;
                await _dbcontext.SaveChangesAsync();
            }
            else
            {
                throw new Exception("courier_unverified_now");
            }

        }

        private (long price, Guid restaurantId) GetPriceAndRestaurantIdFromBasket(List<BasketTable> selectedBasketItems)
        {
            long priceFinal = 0;
            Guid restaurantId = Guid.Empty;

            foreach (var basketItemDb in selectedBasketItems)
            {
                var selectedFoodItem = _dbcontext.restaurantFoodItemsTable.Where(c => c.Id == basketItemDb.food_item_id).FirstOrDefault();

                if (selectedFoodItem != null)
                {
                    if (restaurantId == Guid.Empty)
                        restaurantId = selectedFoodItem.restaurant_id;

                    priceFinal += selectedFoodItem.price;
                }
            }

            return (priceFinal, restaurantId);
        }

        private async Task BasketItemsToOrder(List<BasketTable> selectedBasketItems, Guid orderId)
        {
            foreach (var basketItemDb in selectedBasketItems)
            {
                var selectedFoodItem = _dbcontext.restaurantFoodItemsTable.Where(c => c.Id == basketItemDb.food_item_id).FirstOrDefault();

                if (selectedFoodItem != null)
                {
                    OrderItemsTable orderItem = new OrderItemsTable()
                    {
                        order_id = orderId,
                        restaraunt_food_item = selectedFoodItem.Id
                    };

                    _dbcontext.orderItemsTable.Add(orderItem);
                    await _dbcontext.SaveChangesAsync();
                }
            }
        }

        private async Task<bool> BalanceOperated(Guid userGUID, long price_requested)
        {
            var selectedUser = _dbcontext.userTable.Where(c => c.Id == userGUID).FirstOrDefault();

            if (selectedUser == null)
                throw new Exception("user_not_found");

            if (selectedUser.money_value >= price_requested)
            {
                selectedUser.money_value -= price_requested;
                await _dbcontext.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<Order_DTO> CreateOrder(Guid userGUID)
        {
            var selectedBasketItems = _dbcontext.basketTable.Where(c => c.user_id == userGUID).ToList();

            if (selectedBasketItems.Count == 0)
                throw new Exception("basket_was_empty");

            var selectedClient = _dbcontext.userTable.Where(c => c.Id == userGUID).FirstOrDefault();

            if (selectedClient == null)
                throw new Exception("user_not_found");

            var tupleBasket = GetPriceAndRestaurantIdFromBasket(selectedBasketItems);

            //Проверка, хватает ли денег на балансе или нет. Если хватает то списываем
            if (!(await BalanceOperated(userGUID, tupleBasket.price)))
                throw new Exception("no_money_for_pay");

            OrderTable orderRelease = new OrderTable()
            {
                client_id = userGUID,
                courier_id = null,
                restaurant_id = tupleBasket.restaurantId,
                order_date = DateTime.UtcNow,
                total_price = tupleBasket.price,
                status = OrderStatus.AfterPay,
                client_address = selectedClient.address!
            };

            //Создание заказа
            _dbcontext.orderTable.Add(orderRelease);
            await _dbcontext.SaveChangesAsync();

            //Все айтемы из корзины в заказ
            await BasketItemsToOrder(selectedBasketItems, orderRelease.Id);

            //Очищаем корзину, ведь мы уже заказали
            await DeleteFullBasket(userGUID);

            OrderStatusHistoryTable orderStatusHistory = new OrderStatusHistoryTable()
            {
                order_id = orderRelease.Id,
                status = OrderStatus.AfterPay,
                status_datetime = DateTime.UtcNow
            };
            
            //Записываем в историю что заказ был оплачен
            _dbcontext.orderHistory.Add(orderStatusHistory);
            await _dbcontext.SaveChangesAsync();

            return new Order_DTO()
            {
                id = orderRelease.Id,
                client_id = orderRelease.client_id,
                courier_id = orderRelease.courier_id,
                order_date = orderRelease.order_date,
                restaurant_id = orderRelease.restaurant_id,
                status = orderRelease.status,
                total_price = orderRelease.total_price,
                client_address = orderRelease.client_address
            };
        }

        public OrderInfo GetOrderInfoFromId(Guid orderId)
        {
            OrderInfo_Courier? orderCourier = null;

            List<OrderInfo_Items> orderFoodItems = new List<OrderInfo_Items>();

            var selectedOrder = _dbcontext.orderTable.Where(c => c.Id == orderId).FirstOrDefault();

            if (selectedOrder == null)
                throw new Exception("order_not_found");


            var selectedRestaurant = _dbcontext.restaurantTable
                .Where(c => c.Id == selectedOrder.restaurant_id).FirstOrDefault();

            if (selectedRestaurant == null)
                throw new Exception("restaurant_not_found");

            OrderInfo_Restaurant orderRestaurant = new OrderInfo_Restaurant()
            {
                restaurant_id = selectedRestaurant.Id,
                address = selectedRestaurant.address,
                imagePath = selectedRestaurant.imagePath,
                phone_number = selectedRestaurant.phone_number,
                restaurantName = selectedRestaurant.restaurantName
            };

            var selectedItems = _dbcontext.orderItemsTable.Where(c => c.order_id == orderId).ToList();

            foreach (var foodItem in selectedItems) {
                var itemFromRestaurant = _dbcontext.restaurantFoodItemsTable
                    .Where(c => c.Id == foodItem.restaraunt_food_item).FirstOrDefault();

                if (itemFromRestaurant != null)
                {
                    OrderInfo_Items orderItem = new OrderInfo_Items()
                    {
                        restaurant_id = itemFromRestaurant.restaurant_id,
                        name = itemFromRestaurant.name,
                        calories = itemFromRestaurant.calories,
                        image = itemFromRestaurant.image,
                        price = itemFromRestaurant.price,
                        weight = itemFromRestaurant.weight
                    };

                    orderFoodItems.Add(orderItem);
                }
            }


            var selectedCourier = _dbcontext.courierTable
                .Where(c => c.Id == selectedOrder.courier_id).FirstOrDefault();

            if (selectedCourier != null)
            {
                var selectedCourierUser = _dbcontext.userTable
                    .Where(c => c.Id == selectedCourier.userId).FirstOrDefault();

                if (selectedCourierUser == null)
                    throw new Exception("user_not_found");

                orderCourier = new OrderInfo_Courier()
                {
                    courier_id = selectedCourier.Id,
                    user_id = selectedCourierUser.Id,
                    car_number = selectedCourier.car_number,
                    address = selectedCourierUser.address,
                    chat_id = selectedCourierUser.telegram_chat_id,
                    first_name = selectedCourierUser.first_name,
                    last_name = selectedCourierUser.last_name,
                    photo_url = selectedCourierUser.photo_url,
                    username = selectedCourierUser.username
                };

            }

            var selectedHistoryOrder = _dbcontext.orderHistory.Where(c => c.order_id == selectedOrder.Id).OrderByDescending(x => x.status_datetime).FirstOrDefault();

            var status_order_now = "";

            if (selectedHistoryOrder == null)
                throw new Exception("order_status_history_not_found");
            
            if (selectedHistoryOrder.status == OrderStatus.AfterPay)
                status_order_now = "Заказ оплачен, ожидаем ресторан";

            if (selectedHistoryOrder.status == OrderStatus.Accepted)
                status_order_now = "Заказ принят и готовится";

            if (selectedHistoryOrder.status == OrderStatus.Ready)
                status_order_now = "Заказ готов, поиск курьера";

            if (selectedHistoryOrder.status == OrderStatus.WaitingForDelivery)
                status_order_now = "Курьер найден, осуществляется доставка";

            if (selectedHistoryOrder.status == OrderStatus.CourierOnPlace)
                status_order_now = "Курьер на месте";

            if (selectedHistoryOrder.status == OrderStatus.Delivered)
                status_order_now = "Заказ доставлен и получен";
        

            OrderInfo orderFinal = new OrderInfo()
            {
                courier_info = orderCourier,
                order_date = selectedOrder.order_date,
                order_id = selectedOrder.Id,
                price_order = selectedOrder.total_price,
                restaurant_info = orderRestaurant,
                status_order = status_order_now,
                last_status_change = selectedHistoryOrder.status_datetime,
                food_items = orderFoodItems,
                client_address = selectedOrder.client_address
            };

            return orderFinal;
        }

        public List<OrderInfo> GetAllOrders(Guid userGUID)
        {
            List<OrderInfo> orderInfos = new List<OrderInfo>();

            var selectedOrders = _dbcontext.orderTable.Where(c => c.client_id == userGUID).ToList();

            if (selectedOrders.Count == 0)
                throw new Exception("orders_not_found");

            foreach (var order in selectedOrders)
            {
                var orderInfo = GetOrderInfoFromId(order.Id);

                orderInfos.Add(orderInfo);
            }

            return orderInfos;
        }

        public List<OrderInfo_History> GetHistoryStatusOrder(Guid orderId)
        {
            List<OrderInfo_History> ordersHistory = new List<OrderInfo_History>();

            var selectedHistoryOrders = _dbcontext.orderHistory.Where(c => c.order_id == orderId).OrderBy(x => x.status_datetime).ToList();

            foreach (var historyOrder in selectedHistoryOrders)
            {
                var status_order_now = "Заказ оплачен, ожидаем ресторан";

                if (historyOrder == null)
                    throw new Exception("order_status_history_not_found");

                if (historyOrder.status == OrderStatus.AfterPay)
                    status_order_now = "Заказ оплачен, ожидаем ресторан";

                if (historyOrder.status == OrderStatus.Accepted)
                    status_order_now = "Заказ принят и готовится";

                if (historyOrder.status == OrderStatus.Ready)
                    status_order_now = "Заказ готов, поиск курьера";

                if (historyOrder.status == OrderStatus.WaitingForDelivery)
                    status_order_now = "Курьер найден, осуществляется доставка";

                if (historyOrder.status == OrderStatus.CourierOnPlace)
                    status_order_now = "Курьер на месте";

                if (historyOrder.status == OrderStatus.Delivered)
                    status_order_now = "Заказ доставлен и получен";

                OrderInfo_History orderHistory = new OrderInfo_History()
                {
                    orderId = orderId,
                    status = status_order_now,
                    change_time = historyOrder.status_datetime
                };

                ordersHistory.Add(orderHistory);
            }

            return ordersHistory;
        }

        public async Task ChangeOrAddEmail(string email, Guid userGUID)
        {
            var selectedUser = _dbcontext.userTable
                .Where(c => c.Id == userGUID).FirstOrDefault();

            if (selectedUser == null)
                throw new Exception("user_not_found");

            selectedUser.email = email;
            await _dbcontext.SaveChangesAsync();
        }

        public string GetTelegramChatIdFromRequestId(Guid requestId)
        {
            var selectedRequest = _dbcontext.requestTable.Where(c => c.Id == requestId).FirstOrDefault();

            if (selectedRequest != null)
            {
                var selectedUser = _dbcontext.userTable.Where(c => c.Id == selectedRequest.user_id).FirstOrDefault();

                if (selectedUser != null)
                    return selectedUser.telegram_chat_id.ToString();
            }

            return string.Empty;
        }

        public async Task InsertMoney(Guid userGUID, long money_value)
        {
            var selectedUser = _dbcontext.userTable.Where(c => c.Id == userGUID).FirstOrDefault();

            if (selectedUser == null)
                throw new Exception("user_not_found");

            if (money_value <= 0)
                throw new Exception("inane_money_value");

            selectedUser.money_value += money_value;
            await _dbcontext.SaveChangesAsync();
        }

        public bool ExistMoney(Guid userGUID, long money_value)
        {
            var selectedUser = _dbcontext.userTable.Where(c => c.Id == userGUID).FirstOrDefault();

            if (selectedUser == null)
                throw new Exception("user_not_found");

            return selectedUser.money_value >= money_value;
        }

        public async Task DecreaseMoney(Guid userGUID, long money_value) 
        {
            var selectedUser = _dbcontext.userTable.Where(c => c.Id == userGUID).FirstOrDefault();

            if (selectedUser == null)
                throw new Exception("user_not_found");

            if (selectedUser.money_value - money_value < 0)
                throw new Exception("inane_money_value");

            selectedUser.money_value -= money_value;
            await _dbcontext.SaveChangesAsync();
        }

        public string GetTelegramChatId(Guid userGUID)
        {
            var selectedUser = _dbcontext.userTable.Where(c => c.Id == userGUID).FirstOrDefault();

            if (selectedUser != null)
                return selectedUser.telegram_chat_id.ToString();
    
            return string.Empty;
        }

        public long GetUserBalance(Guid userGUID)
        {
            var selectedUser = _dbcontext.userTable.Where(c => c.Id == userGUID).FirstOrDefault();

            if (selectedUser != null)
                return selectedUser.money_value;

            return (long)0;
        }

        public async Task<List<ReviewDto>> GetAllReviews()
        {
            return await _dbcontext.reviewTable
                    .Select(x => new ReviewDto(
                        x.Id, x.order_id,
                        x.client_id, x.courier_id,
                        x.rating, x.comment,
                        x.review_date))
                    .ToListAsync();
        }

        public async Task CreateReview(ReviewTable review)
        {
            _dbcontext.Add(review);
            await _dbcontext.SaveChangesAsync();
        }

        public async Task UpdateReview(Guid reviewId, ReviewDtoForUpdate reviewUpdateDto)
        {
            var review = await _dbcontext.reviewTable
                .FirstOrDefaultAsync(x => x.Id == reviewId)
                ?? throw new Exception($"Отзыв {reviewId} не найден.");
            
            review.rating = reviewUpdateDto.rating ?? 5;
            review.comment = reviewUpdateDto.comment;
            review.review_date = DateTime.UtcNow;

            await _dbcontext.SaveChangesAsync();
        }
    }
}
