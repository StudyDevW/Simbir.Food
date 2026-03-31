namespace ORM_Components.DTO.ClientAPI
{
    /// <summary>
    ///  Переменные взяты из API Mini Apps VK
    ///  https://dev.vk.com/ru/bridge/VKWebAppGetUserInfo
    /// </summary>
    public class ClientUpdate
    {
        public long id { get; set; }

        public string first_name { get; set; }

        public string last_name { get; set; }

        public string? photo_max_orig { get; set; }

        public string? address { get; set; }
    }
}
