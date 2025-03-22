namespace ORM_Components.DTO.RestaurantAPI
{
    public record RestaurantFoodItemsDto(
        Guid Id, 
        Guid restaurant_id, string name, 
        long price, string image,
        int weight, int calories) 
    { }

    public record RestaurantFoodItemsDtoForCreate(
        Guid restaurant_id, string name,
        long price, string image,
        int weight, int calories)
    { }

    public record RestaurantFoodItemsDtoForUpdate(
        string? name,
        long? price, string? image,
        int? weight, int? calories)
    { }
}
