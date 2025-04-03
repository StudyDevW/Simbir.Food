using ORM_Components.DTO.ClientAPI;
using ORM_Components.DTO.RestaurantAPI;

namespace RestaurantAPI.Model.Interface
{
    public interface IRestaurantServices
    {
        Task<Restaurants_DTO> GetRestaurant(Guid restaurantId);
        Task<List<Restaurants_DTO>> GetAllRestaurant(string bearerKey, string? search);
        Task UpdateRestaurant(Guid restaurantId, RestaurantUpdate_DTO restaurantUpdate_DTO);
        Task DeleteRestaurant(Guid restaurantId);
        Task DeleteAllRestaurant();
        Task<List<RestaurantMark_DTO>> GetRestaurantMark();
        Task SetReadyStatusForOrder(Guid orderId);
        Task OrderRejections(Guid orderId);

    }

}
