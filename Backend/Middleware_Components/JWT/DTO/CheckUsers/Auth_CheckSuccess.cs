using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Middleware_Components.JWT.DTO.CheckUsers
{
    public class Auth_CheckSuccess
    {
        public Guid Id { get; set; }

        public string? login { get; set; }

        public string telegramChatId { get; set; }

        public List<string>? roles { get; set; }
    }
}
