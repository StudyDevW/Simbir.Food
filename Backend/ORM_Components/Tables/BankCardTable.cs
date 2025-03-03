using ORM_Components.Tables.Helpers;

namespace ORM_Components.Tables
{
    public class BankCardTable : IId
    {
        public string card_number { get; set; }

        public string cvv { get; set; }

        public long money_value { get; set; }

        public string name_card { get; set; }
    }
}
