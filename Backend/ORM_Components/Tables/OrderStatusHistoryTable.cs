using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM_Components.Tables
{
    public class OrderStatusHistoryTable
    {
        [Key]
        public int id { get; set; }

        public int order_id { get; set; }

        public string status { get; set; }

        public DateTime status_datetime { get; set; }
    }
}
