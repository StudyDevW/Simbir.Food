using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM_Components.DTO.RestaurantAPI
{
    public class RestaurantFoodItems_DTO
    {
        public Guid restaurant_id { get; set; }

        public string name { get; set; }

        public int price { get; set; }

        public string image { get; set; }

        public int weight { get; set; }

        public int calories { get; set; }
    }
}
