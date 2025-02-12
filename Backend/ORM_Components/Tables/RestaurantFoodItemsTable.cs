using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM_Components.Tables
{
    public class RestaurantFoodItemsTable
    {
        [Key]
        public int id { get; set; }

        public int restaurant_id { get; set; }

        public string name { get; set; }

        public int price { get; set; }

        public string image { get; set; }

        public int weight { get; set; }

        public int calories { get; set; }
    }
}
