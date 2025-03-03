namespace ORM_Components.DTO.PaymentAPI
{
    public class Payment_Release
    {
        public Guid user_id { get; set; }

        public string card_number { get; set; }

        public string cvv { get; set; }

        public long money_value { get; set; }

        public bool link_card { get; set; }
    }
}
