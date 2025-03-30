using Middleware_Components.JWT.DTO.CheckUsers;
using ORM_Components.DTO.ClientAPI;
using ORM_Components.DTO.ClientAPI.Basket;
using ORM_Components.DTO.ClientAPI.ClientsAll;
using ORM_Components.DTO.ClientAPI.FrozenAll;
using ORM_Components.DTO.ClientAPI.OrderSelecting;
using ORM_Components.DTO.ClientAPI.RequestsAll;
using ORM_Components.DTO.ClientAPI.Review;
using ORM_Components.Tables;

namespace ClientAPI.Interfaces
{
    public interface IDatabaseService
    {
        public Task UserUpdateFromTelegram(ClientUpdate dtoObj);

        public Auth_CheckInfo CheckUser(AuthSignIn dto);

        public ClientInfo? InfoClientDatabase(Guid userGUID);

        public ClientGetAll GetAllClients(int _from, int _count);

        public Task AddBasketItem(Guid fooditemId, Guid userGUID);

        public Task<Basket_GetAll> GetBasketItems(Guid userGUID);

        public Task DeleteClientWithAdmin(Guid id);

        public Task DeleteAllBasketWrites(Guid userGUID);

        public Task DeleteOneBasketWrite(Guid userGUID, Guid basketId);

        public Task<Guid> CreateRequestRestaurantFromUser(Guid userGUID, RestaurantAddRequest dtoObj);

        public Task<Guid> CreateRequestCourierFromUser(Guid userGUID, string? car_number, string description);

        public Task AcceptRequestRestaurantFromAdmin(Guid requestId);

        public Task AcceptRequestCourierFromAdmin(Guid requestId);

        public Task RejectRequestRestaurantFromAdmin(Guid requestId);

        public Task RejectRequestCourierFromAdmin(Guid requestId);

        public RequestsGetAll GetAllRequestsForAdmin();

        public RequestInfo_Couriers? GetOnlyMeRequestCourier(Guid user_id);

        public List<RequestInfo_Restaurants> GetOnlyMeRequestsRestaurant(Guid user_id);

        public Task<Guid> FreezeRestaurantWork(Guid restaurantId);

        public Task FreezeCourierWork(Guid userGUID);

        public Task<Guid> UnfreezeRestaurantWork(Guid restaurantId);

        public Task UnfreezeCourierWork(Guid userGUID);

        public FrozenGetAll GetAllFrozenEntities();

        public Task<Order_DTO> CreateOrder(Guid userGUID);

        public OrderInfo GetOrderInfoFromId(Guid orderId);

        public List<OrderInfo> GetAllOrders(Guid userGUID);

        public List<OrderInfo_History> GetHistoryStatusOrder(Guid orderId);

        public Task ChangeOrAddEmail(string email, Guid userGUID);

        public Task InsertMoney(Guid userGUID, long money_value);

        public bool ExistMoney(Guid userGUID, long money_value);

        public Task DecreaseMoney(Guid userGUID, long money_value);

        public string GetTelegramChatIdFromRequestId(Guid requestId);

        public string GetTelegramChatId(Guid userGUID);

        public long GetUserBalance(Guid userGUID);
        
        public Task<List<ReviewDto>> GetAllReviews();

        public Task CreateReview(ReviewTable review);

        public Task UpdateReview(Guid reviewId, ReviewDtoForUpdate reviewDtoUpdate);
    }
}
