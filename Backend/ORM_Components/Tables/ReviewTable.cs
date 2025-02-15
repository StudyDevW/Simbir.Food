using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ORM_Components.Tables.Helpers;

namespace ORM_Components.Tables
{
    public class ReviewTable : IId
    {
        public Guid order_id { get; set; }

        public Guid client_id { get; set; }

        public Guid? courier_id { get; set; }

        public Guid? restaurant_id { get; set; }

        public int rating { get; set; }

        public string? comment { get; set; }

        public DateTime review_date { get; set; }
    }
}
