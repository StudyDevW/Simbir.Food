using ORM_Components.DTO.ClientAPI.RequestsAll;

namespace ORM_Components.DTO.ClientAPI.FrozenAll
{
    public class FrozenGetAll
    {
        public List<FrozenInfo_Restaurants> frozen_restaurants {  get; set; }

        public List<FrozenInfo_Couriers> frozen_couriers { get; set; }

        public void RestaurantFill(List<FrozenInfo_Restaurants> listOut)
        {
            frozen_restaurants = listOut;
        }

        public void CourierFill(List<FrozenInfo_Couriers> listOut)
        {
            frozen_couriers = listOut;
        }
    }
}
