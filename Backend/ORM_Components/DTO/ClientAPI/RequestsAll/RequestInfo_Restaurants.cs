namespace ORM_Components.DTO.ClientAPI.RequestsAll
{
    public class RequestInfo_Restaurants
    {
        public Guid request_id { get; set; }

        public string restaurantName { get; set; }

        public string address { get; set; }

        public string phone_number { get; set; }

        public string description { get; set; }

        public string imagePath { get; set; }

        public string open_time { get; set; }

        public string close_time { get; set; }

        public string request_description { get; set; }

        public DateTime request_time_add { get; set; }

        public RequestClientInfo client_info { get; set; }
    }
}
