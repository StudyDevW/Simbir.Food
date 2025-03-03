namespace ORM_Components.DTO.ClientAPI
{
    /// <summary>
    ///  Переменные взяты из API Mini Apps Telegram
    ///  https://docs.telegram-mini-apps.com/platform/init-data#user
    /// </summary>
    public class ClientUpdate
    {
        /// <summary>
        /// Bot or user ID.
        /// </summary>
        public long id { get; set; }

        /// <summary>
        /// Bot or user name.
        /// </summary>
        public string first_name { get; set; }

        /// <summary>
        /// Optional. User's last name.
        /// </summary>
        public string? last_name { get; set; }

        /// <summary>
        /// Optional. Login of the bot or user.
        /// </summary>
        public string? username { get; set; }

        /// <summary>
        /// Optional. Link to the user's or bot's photo.
        /// Photos can have formats.jpeg and.svg.
        /// It is returned only for Mini Apps opened through
        /// the attachment menu.
        /// </summary>
        public string? photo_url { get; set; }

        /// <summary>
        /// User address (кастомная переменная)
        /// </summary>
        public string? address { get; set; }
    }
}
