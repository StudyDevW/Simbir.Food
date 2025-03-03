namespace ORM_Components.DTO.PaymentAPI
{
    public class Payment_Out_Queue
    {
        public string card_number { get; set; }

        public long money_value { get; set; }

        public Guid user_id { get; set; }
    }
}
