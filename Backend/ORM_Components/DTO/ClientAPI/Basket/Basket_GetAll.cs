namespace ORM_Components.DTO.ClientAPI.Basket
{
    public class Basket_GetAll
    {
        public Basket_GetAll_Info? basketInfo { get; set; }

        public List<Basket_GetAll_Item>? basketItem { get; set; }

    }
}
