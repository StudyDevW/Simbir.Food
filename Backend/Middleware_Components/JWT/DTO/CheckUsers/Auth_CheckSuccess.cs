using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Middleware_Components.JWT.DTO.CheckUsers
{
    public class Auth_CheckSuccess
    {
        public int Id { get; set; }

        public string? username { get; set; }

        public List<string>? roles { get; set; }
    }
}
