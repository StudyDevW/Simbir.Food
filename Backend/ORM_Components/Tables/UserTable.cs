using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ORM_Components.Tables.Helpers;

namespace ORM_Components.Tables
{
    public class UserTable : IId
    {
        public string first_name { get; set; }

        public string? last_name { get; set; }

        public int telegram_id { get; set; }

        public int telegram_chat_id { get; set; }

        public string? address { get; set; }

        public string? photo_url { get; set; }

        public string? username { get; set; }

        public string[] roles { get; set; }
    }
}
