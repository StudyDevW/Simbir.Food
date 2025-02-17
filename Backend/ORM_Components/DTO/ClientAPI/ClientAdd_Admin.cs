namespace ORM_Components.DTO.ClientAPI
{
    public class ClientAdd_Admin
    {
        public string name { get; set; }

        public string phone_number { get; set; }

        public string address { get; set; }

        public string email { get; set; }

        public string? avatarImage { get; set; }

        public string login { get; set; }

        public string password { get; set; }

        public string[] roles { get; set; }
    }
}
