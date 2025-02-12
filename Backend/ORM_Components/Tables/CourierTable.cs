using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM_Components.Tables
{
    public class CourierTable
    {
        [Key]
        public int id { get; set; }

        public int userId { get; set; }

        public string? car_number { get; set; }

        public string status { get; set; }
    }

}
