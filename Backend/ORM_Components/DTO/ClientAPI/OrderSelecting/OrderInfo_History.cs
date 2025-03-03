namespace ORM_Components.DTO.ClientAPI.OrderSelecting
{
    public class OrderInfo_History
    {
        public Guid orderId { get; set; }

        public string status { get; set; }

        public DateTime? change_time { get; set; }
    }
}
