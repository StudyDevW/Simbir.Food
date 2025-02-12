using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM_Components.Tables
{
    public class ReviewTable
    {
        [Key]
        public int id { get; set; }

        public int order_id { get; set; }

        public int client_id { get; set; }

        public int? courier_id { get; set; }

        public int? restaurant_id { get; set; }

        public int rating { get; set; }

        public string? comment { get; set; }

        public DateTime review_date { get; set; }
    }
}
