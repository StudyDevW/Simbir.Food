using ORM_Components.DTO.CourierAPI;

namespace CourierAPI.Contracts
{
    public interface ICourierService
    {
        Task<List<OrderForCourierDto>> GetOrders();
        Task AcceptOrder(OrderLinkCourierDto orderLinkCourierDto);
        Task TakeOrder(Guid orderId);
        Task CourierOnPlace(Guid orderId);
        Task OrderDelivered(Guid orderId);

        Task<CourierDto> GetAsync(Guid courierId);
        Task<List<CourierDto>> GetAllAsync();
        Task CreateAsync(CourierDtoForCreate courierDtoForCreate);
        Task UpdateAsync(CourierDtoForUpdate courierDtoForUpdate);
        Task DeleteAsync(Guid courierId);

        Task TestMethod();
    }
}
