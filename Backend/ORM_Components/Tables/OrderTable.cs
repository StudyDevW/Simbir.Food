using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ORM_Components.Tables.Helpers;

namespace ORM_Components.Tables
{
    public class OrderTable : IId
    {
        public Guid client_id { get; set; }

        public Guid restaurant_id { get; set; }

        public Guid? courier_id { get; set; }

        public OrderStatus status { get; set; }

        public int total_price { get; set; }

        public DateTime order_date { get; set; }
    }
}
