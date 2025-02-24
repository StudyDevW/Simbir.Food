using ORM_Components.Tables.Helpers;

namespace ORM_Components.DTO.RestaurantAPI
{
    public record OrderIdsDto(Guid Id, Guid clientId, Guid restaurant_id, Guid courier_id) { }
}
