using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM_Components.Tables.Helpers
{
    public abstract class IId
    {
        [Key]
        public Guid Id { get; set; }
    }
}