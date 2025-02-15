using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ORM_Components.Tables.Helpers;

namespace ORM_Components.Tables
{
    public class CourierTable : IId
    {
        public Guid userId { get; set; }

        public string? car_number { get; set; }

        public string status { get; set; }
    }

}
