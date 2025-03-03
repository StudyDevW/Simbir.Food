using ORM_Components.Tables.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM_Components.DTO.ClientAPI
{
    public class RestaurantAddRequest
    {
        public string restaurantName { get; set; }

        public string address { get; set; }

        public string phone_number { get; set; }

        public string description { get; set; }

        public string imagePath { get; set; }

        public string open_time { get; set; }

        public string close_time { get; set; }

        public string request_description { get; set; }
    }
}
