namespace ORM_Components.DTO.ClientAPI.OrderSelecting
{
    public class OrderInfo_Courier
    {
        public Guid courier_id { get; set; }

        public Guid user_id { get; set; }

        public string first_name { get; set; }

        public string? last_name { get; set; }

        public string? username { get; set; }

        public string? photo_url { get; set; }

        public string? car_number { get; set; }

        public long chat_id { get; set; }

        public string? address { get; set; }

    }
}
