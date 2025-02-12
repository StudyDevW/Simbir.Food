using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM_Components.Tables
{
    public class UserTable
    {
        [Key]
        public int id { get; set; }

        public string name { get; set; }

        public string phone_number { get; set; }

        public string address { get; set; }

        public string email { get; set; }

        public string? avatarImage { get; set; }

        public string login { get; set; }

        public string password { get; set; }

        public string[] roles { get; set; }
    }
}
