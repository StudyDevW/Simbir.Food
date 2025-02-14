using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM_Components.DTO.RestaurantAPI
{
    internal class RestaurantFoodItems
    {
        public Guid id { get; set; }
        public int restaraunt_id { get; set; }
        public string name { get; set; }
        public string price { get; set; }
        public string? Img { get; set; }
        public string weight { get; set; }
        public string calories { get; set; }
    }
}
