namespace ORM_Components.DTO.ClientAPI.FrozenAll
{
    public class FrozenInfo_Restaurants
    {
        public Guid restaurantId { get; set; }

        public Guid user_id { get; set; }

        public string restaurantName { get; set; }

        public string address { get; set; }

        public string imagePath { get; set; }
    }
}
