namespace ORM_Components.DTO.ClientAPI.OrderSelecting
{
    public class OrderInfo
    {
        public Guid order_id { get; set; }

        public string status_order { get; set; }

        public long price_order { get; set; }

        public DateTime order_date { get; set; }

        public DateTime? last_status_change { get; set; }

        public OrderInfo_Courier? courier_info { get; set; }

        public OrderInfo_Restaurant restaurant_info { get; set; }

        public List<OrderInfo_Items> food_items { get; set; }
    }
}
