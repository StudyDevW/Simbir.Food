using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM_Components.DTO.ClientAPI
{
    public class AuthSignIn
    {
        public int telegram_chat_id { get; set; }

        public string device { get; set; }
    }
}
