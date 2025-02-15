using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Middleware_Components.JWT.DTO.Token
{
    public class Token_ValidSuccess
    {
        public Guid Id { get; set; }

        public string? userName { get; set; }

        public List<string>? userRoles { get; set; }

        public string? bearerWithoutPrefix { get; set; }
    }
}
