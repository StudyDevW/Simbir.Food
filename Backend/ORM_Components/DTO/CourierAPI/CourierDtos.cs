using ORM_Components.Tables.Helpers;

namespace ORM_Components.DTO.CourierAPI
{
    public record CourierDto(Guid Id, Guid userId, string? car_number, CourierStatus status) { }
    public record CourierDtoForCreate(Guid userId, string? car_number) { }
    public record CourierDtoForUpdate(Guid Id, string? car_number, CourierStatus? status) { }
}
