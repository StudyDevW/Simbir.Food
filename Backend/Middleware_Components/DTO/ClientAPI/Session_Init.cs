using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Middleware_Components.DTO.ClientAPI
{
    public class Session_Init
    {
        public DateTime? timeAdd { get; set; }

        public DateTime? timeUpd { get; set; }

        public DateTime? timeDel { get; set; }

        public string tokenSession { get; set; }

        public string statusSession { get; set; }
    }
}
