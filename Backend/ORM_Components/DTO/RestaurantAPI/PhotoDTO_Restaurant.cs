namespace ORM_Components.DTO.RestaurantAPI
{
    public class PhotoDTO_Restaurant
    {
        public Guid restaurantId { get; set; }

        public IFormFile File { get; set; }
    }
}
