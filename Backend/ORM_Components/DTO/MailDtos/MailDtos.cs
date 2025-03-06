using ORM_Components.DTO.RestaurantAPI;

namespace ORM_Components.DTO.MailDtos
{
    public record EmailDto(Guid orderId, string email) { }
}
