using ORM_Components.Tables.Helpers;

namespace ORM_Components.DTO.ClientAPI
{
    public class Order_DTO
    {
        public Guid id { get; set; }
        public Guid client_id { get; set; }
        public Guid restaurant_id { get; set; }
        public Guid courier_id { get; set; }
        public OrderStatus status { get; set; }
        public int total_price { get; set; }
        public DateTime order_date { get; set; }
    }
}
