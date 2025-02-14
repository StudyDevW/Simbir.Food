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
    }
}
