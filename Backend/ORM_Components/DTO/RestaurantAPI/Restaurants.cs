using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM_Components.DTO.RestaurantAPI
{
    public class Restaurants
    {
        public Guid id { get; set; }

        public string    { get; set; }

        public string? Img { get; set; }

        public string description { get; set; }

        public string phone_number { get; set; }

        public string address { get; set; }

        public string status { get; set; }

        public string login { get; set; }

        public string password { get; set; }
    }
}
