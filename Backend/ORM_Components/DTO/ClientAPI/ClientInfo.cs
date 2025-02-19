namespace ORM_Components.DTO.ClientAPI
{
    public class ClientInfo
    {
        public Guid Id { get; set; }

        public string name { get; set; }

        public string phone_number { get; set; }

        public string address { get; set; }

        public string email { get; set; }

        public string avatarImage { get; set; }

        public string login { get; set; }

        public List<string> roles { get; set; }
    }
}
