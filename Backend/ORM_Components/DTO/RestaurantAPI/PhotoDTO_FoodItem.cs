namespace ORM_Components.DTO.RestaurantAPI
{
    public class PhotoDTO_FoodItem
    {
        public Guid fooditemId { get; set; }

        public IFormFile File { get; set; }
    }
}
