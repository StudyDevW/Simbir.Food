using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM_Components.DTO.ClientAPI
{
    public class AuthSignUp
    {
        public string name { get; set; }

        public string address { get; set; }

        public string phone_number { get; set; }

        public string email { get; set; }

        public string login { get; set; }

        public string password { get; set; }

        public string telegram_chatid { get; set; }
    }
}
