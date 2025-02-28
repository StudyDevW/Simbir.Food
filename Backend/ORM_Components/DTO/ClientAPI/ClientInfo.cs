namespace ORM_Components.DTO.ClientAPI
{
    public class ClientInfo
    {
        public Guid Id { get; set; }

        public long telegram_id { get; set; }

        public string first_name { get; set; }

        public string? last_name { get; set; }

        public string? username { get; set; }

        public string? photo_url { get; set; }

        public long chat_id { get; set; }

        public List<Guid>? restaurant_own { get; set; }

        public string? address { get; set; }

        public List<string> roles { get; set; }
    }
}
