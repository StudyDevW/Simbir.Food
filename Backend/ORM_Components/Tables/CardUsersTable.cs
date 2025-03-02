using ORM_Components.Tables.Helpers;

namespace ORM_Components.Tables
{
    public class CardUsersTable : IId
    {
        public Guid user_id { get; set; }

        public string card_number { get; set; }

        public string cvv { get; set; }

        public long money_value { get; set; }
    }
}
