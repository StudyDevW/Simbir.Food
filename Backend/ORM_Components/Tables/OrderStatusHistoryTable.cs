using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ORM_Components.Tables.Helpers;

namespace ORM_Components.Tables
{
    public class OrderStatusHistoryTable : IId
    {
        public Guid order_id { get; set; }

        public OrderStatus status { get; set; }

        public DateTime status_datetime { get; set; }
    }
}
