using ORM_Components.Tables.Helpers;

namespace ORM_Components.Tables
{
    public class RequestTable : IId
    {
        public Guid? restaurant_id { get; set; }

        public Guid? courier_id { get; set; }

        public Guid user_id { get; set; }

        public DateTime time_add { get; set; }

        public string description { get; set; }
    }
}
