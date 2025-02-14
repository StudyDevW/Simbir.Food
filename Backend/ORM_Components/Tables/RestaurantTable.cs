using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ORM_Components.Tables.Helpers;

namespace ORM_Components.Tables
{
    public class RestaurantTable : IId
    {
        public Guid user_id { get; set; }

        public string restaurantName { get; set; }

        public string address { get; set; }

        public string phone_number { get; set; }

        public string status { get; set; }

        public string description { get; set; }

        public string imagePath { get; set; }
    }
}
