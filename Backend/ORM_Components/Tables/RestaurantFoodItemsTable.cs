using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ORM_Components.Tables.Helpers;

namespace ORM_Components.Tables
{
    public class RestaurantFoodItemsTable : IId
    {
        public Guid restaurant_id { get; set; }

        public string name { get; set; }

        public long price { get; set; }

        public string image { get; set; }

        public int weight { get; set; }

        public int calories { get; set; }
    }
}