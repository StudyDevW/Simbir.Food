using FluentValidation;
using ORM_Components.DTO.CourierAPI;

namespace CourierAPI.Contracts
{
    public interface ICourierService
    {
        Task<List<OrderForCourierDto>> GetOrders();
        Task AcceptOrder(Guid orderId);
        Task CourierOnPlace(Guid orderId);
        Task OrderDelivered(Guid orderId);

        Task<CourierDto> GetAsync();
        Task<List<CourierDto>> GetAllAsync();
        Task CreateAsync(CourierDtoForCreate courierDtoForCreate);
        Task UpdateAsync(CourierDtoForUpdate courierDtoForUpdate);
        Task DeleteAsync(Guid courierId);
    }
}
