using ORM_Components.Tables.Helpers;

namespace ORM_Components.Tables
{
    public class BasketTable : IId
    {
        public Guid user_id { get; set; }

        public Guid food_item_id { get; set; }

        //Добавить время добавления
    }
}
