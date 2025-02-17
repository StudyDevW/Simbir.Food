using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM_Components.DTO.RestaurantAPI
{
    public class Restaurants_DTO
    {
        public Guid id { get; set; }

        public Guid user_id { get; set; }

        public string restaurantName { get; set; }

        public string? imagePath { get; set; }

        public string description { get; set; }

        public string phone_number { get; set; }

        public string address { get; set; }

        public string status { get; set; }
        public DateTime open_time { get; set; }
        public DateTime close_time { get; set; }

    }
}
