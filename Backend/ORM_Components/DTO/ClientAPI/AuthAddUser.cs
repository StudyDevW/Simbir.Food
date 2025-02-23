using Pipelines.Sockets.Unofficial.Buffers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM_Components.DTO.ClientAPI
{

    /// <summary>
    ///  Переменные взяты из API Mini Apps Telegram
    ///  https://docs.telegram-mini-apps.com/platform/init-data#user
    /// </summary>
    public class AuthAddUser 
    {
        /// <summary>
        /// Bot or user ID.
        /// </summary>
        public int id { get; set; }

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
        /// Optional. Is the user a bot.
        /// </summary>
        public bool is_bot { get; set; }

        /// <summary>
        /// Optional. Link to the user's or bot's photo.
        /// Photos can have formats.jpeg and.svg.
        /// It is returned only for Mini Apps opened through
        /// the attachment menu.
        /// </summary>
        public string? photo_url { get; set; }

        /// <summary>
        /// Unique chat ID.
        /// </summary>
        public int chat_id { get; set; }

        /// <summary>
        /// User address (кастомная переменная)
        /// </summary>
        public string? address { get; set; }

        /// <summary>
        /// User device (кастомная переменная)
        /// </summary>
        public string device { get; set; }

        /// <summary>
        /// User roles (может изменять только админ)
        /// </summary>
        public string[]? roles { get; set; }
    }
}
