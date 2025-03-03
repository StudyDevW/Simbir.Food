using ORM_Components.Tables.Helpers;

namespace ORM_Components.Tables
{
    public class PayTable : IId
    {
        public Guid user_id { get; set; }

        public PayStatus pay_status { get; set; }

        public DateTime date { get; set; }

        public string card_number { get; set; }
    }
}
