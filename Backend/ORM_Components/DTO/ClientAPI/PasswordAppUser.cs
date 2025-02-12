using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM_Components.DTO.ClientAPI
{
    public class PasswordAppUser
    {
        public string login { get; set; }

        public string passwordHashed { get; set; }
    }
}
