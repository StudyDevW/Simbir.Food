namespace ORM_Components.DTO.ClientAPI.OrderSelecting
{
    public class OrderInfo_Items
    {
        public Guid restaurant_id { get; set; }

        public string name { get; set; }

        public long price { get; set; }

        public string image { get; set; }

        public int weight { get; set; }

        public int calories { get; set; }
    }
}
