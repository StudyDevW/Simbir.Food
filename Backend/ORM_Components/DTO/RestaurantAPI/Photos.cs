using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM_Components.DTO.RestaurantAPI
{
    public class Photos
    {
        public Guid id { get; set; }
        public string Title { get; set; }
        public IFormFile File { get; set; }
        public string Description { get; set; }
    }
}
