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

        public string restaurantName { get; set; } = string.Empty;

        public string address { get; set; } = string.Empty;

        public string phone_number { get; set; } = string.Empty;

        public RestaurantStatus status { get; set; } = RestaurantStatus.Unverified;

        public string description { get; set; } = string.Empty;

        public string imagePath { get; set; } = string.Empty;

        public string open_time { get; set; } = string.Empty;

        public string close_time { get; set; } = string.Empty;
    }
}