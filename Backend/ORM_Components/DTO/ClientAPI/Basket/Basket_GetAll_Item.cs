namespace ORM_Components.DTO.ClientAPI.Basket
{
    public class Basket_GetAll_Item
    {
        public Guid id { get; set; }

        public Guid restaurant_id { get; set; }

        public string name { get; set; }

        public long price { get; set; }

        public string image { get; set; }

        public int weight { get; set; }

        public int calories { get; set; }
    }
}
