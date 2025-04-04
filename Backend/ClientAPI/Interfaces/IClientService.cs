using Middleware_Components.DTO.ClientAPI;
using ORM_Components.DTO.ClientAPI;
using ORM_Components.DTO.ClientAPI.Basket;
using ORM_Components.DTO.ClientAPI.ClientsAll;
using ORM_Components.DTO.ClientAPI.FrozenAll;
using ORM_Components.DTO.ClientAPI.OrderSelecting;
using ORM_Components.DTO.ClientAPI.RequestsAll;
using ORM_Components.DTO.ClientAPI.Review;
using ORM_Components.DTO.PaymentAPI;
using ORM_Components.DTO.RestaurantAPI;

namespace ClientAPI.Interfaces
{
    public interface IClientService
    {
        public Task<Auth_PairTokens?> UserAuth(AuthAddUser dtoObj);

        public Task<string> UserRegister(AuthAddUser dtoObj);

        public Task<string?> ClientSignOut(string bearer_key);

        public Task<Auth_PairTokens?> RefreshClientSession(Auth_RefreshTokens dtoObj);

        public Task<ClientInfo?> ClientMeInfo(string bearer_key);

        public Task<ClientInfo?> ClientFromIdInfo(string bearer_key, Guid userGUID);

        public Task<ClientGetAll?> AllProfilesGet(string bearer_key, int from, int count);

        //public Task UpdateClientInfo(string bearer_key, ClientUpdate dtoObj);

        //public Task UpdateClientInfoWithAdmin(string bearer_key, ClientUpdate_Admin dtoObj, Guid userGUID);

        //public Task CreateClientWithAdmin(string bearer_key, ClientAdd_Admin dtoObj);

        public Task DeleteClientWithAdmin(string bearer_key, Guid userGUID);

        public Task<Basket_GetAll?> GetItemsBasket(string bearer_key);

        public Task AddBasketItem(string bearer_key, Basket_Add dtoObj);

        public Task DeleteAllBasket(string bearer_key);

        public Task DeleteOneBasketItem(string bearer_key, Guid basketId);

        public Task<RequestsGetAll?> GetAllRequestsForAdmin(string bearer_key);

        public Task CreateRestaurantRequest(string bearer_key, RestaurantAddRequest dtoObj);

        public Task CreateCourierRequest(string bearer_key, string? car_number, string description);

        public Task<List<RequestInfo_Restaurants>?> GetMeRequestsRestaurant(string bearer_key);

        public Task<RequestInfo_Couriers?> GetMeRequestCourier(string bearer_key);

        public Task AcceptRequests(string bearer_key, Guid requestId, string type);

        public Task RejectRequests(string bearer_key, Guid requestId, string type);

        public Task FreezeWorkRestaurantWithAdmin(string bearer_key, Guid restaurantId, Downgrade dtoObj);

        public Task FreezeWorkCourierWithAdmin(string bearer_key, Guid userGUID, Downgrade dtoObj);

        public Task UnfreezeRestaurantWithAdmin(string bearer_key, Guid restaurantId);

        public Task UnfreezeCourierWithAdmin(string bearer_key, Guid userGUID);

        public Task<FrozenGetAll?> GetAllFrozenEntities(string bearer_key);

        public Task CreateOrder(string bearer_key);

        public Task<OrderInfo?> GetOrderFromId(string bearer_key, Guid orderId);

        public Task<List<OrderInfo>?> GetAllOrders(string bearer_key);

        public Task<List<OrderInfo_History>?> GetAllHistoryOrder(string bearer_key, Guid orderId);

        public Task MoneyOut(string bearer_key, PaymentOut dtoObj);

        public Task ChangeOrAddEmail(string bearer_key, string email);

        public Task<List<ReviewDto>> GetAllReviews(string bearer_key);

        public Task UpdateReview(string bearer_key, Guid reviewId, ReviewDtoForUpdate reviewUpdateDto);

        public Task<List<RestaurantDTOForOwnerList>> GetAllUserRestaurants(string bearer_key);

        public Task<List<OrderInfo>> GetAllOrdersForRestaurant(string bearer_key, Guid restaurantId, bool isNeedAllOrders);

        public Task AddRestaurantToFavourite(string bearer_key, Guid RestaurantId);

        public Task RemoveRestaurantFromFavourite(string bearer_key, Guid RestaurantId);

    }
}
