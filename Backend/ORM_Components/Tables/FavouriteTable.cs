using ORM_Components.Tables.Helpers;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ORM_Components.Tables
{
    public class FavouriteTable
    {
        public Guid UserId { get; set; }

        public Guid RestaurantId { get; set; }
    }
}
