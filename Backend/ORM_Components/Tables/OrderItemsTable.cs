using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM_Components.Tables
{
    public class OrderItemsTable
    {
        [Key]
        public int id { get; set; }

        public int order_id { get; set; }   

        public int restaraunt_food_item { get; set; }

        public int price { get; set; }

        public int quantity { get; set; }
    }
}
