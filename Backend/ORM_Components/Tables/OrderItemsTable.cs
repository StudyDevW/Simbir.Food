using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ORM_Components.Tables.Helpers;

namespace ORM_Components.Tables
{
    public class OrderItemsTable : IId
    {
        public Guid order_id { get; set; }   

        public int restaraunt_food_item { get; set; }

        public int price { get; set; }

        public int quantity { get; set; }
    }
}
