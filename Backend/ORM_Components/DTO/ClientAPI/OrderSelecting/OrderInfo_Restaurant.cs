namespace ORM_Components.DTO.ClientAPI.OrderSelecting
{
    public class OrderInfo_Restaurant
    {
        public Guid restaurant_id { get; set; }

        public string restaurantName { get; set; }

        public string address { get; set; }

        public string phone_number { get; set; }

        public string imagePath { get; set; }
    }
}
