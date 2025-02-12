using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM_Components.Tables
{
    public class OrderTable
    {
        [Key]
        public int id { get; set; }

        public int client_id { get; set; }

        public int restaurant_id { get; set; }

        public int? courier_id { get; set; }

        public string status { get; set; }

        public int total_price { get; set; }

        public DateTime order_date { get; set; }
    }
}
