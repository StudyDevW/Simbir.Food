namespace ORM_Components.DTO.ClientAPI.RequestsAll
{
    public class RequestInfo_Couriers
    {
        public Guid request_id { get; set; }

        public string? car_number { get; set; }

        public string request_description { get; set; }

        public DateTime request_time_add { get; set; }

        public RequestClientInfo client_info { get; set; }
    }
}
