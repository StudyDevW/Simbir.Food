namespace ORM_Components.DTO.ClientAPI.RequestsAll
{
    public class RequestsGetAll
    {
        public List<RequestInfo_Restaurants> restaurant_requests { get; set; }

        public List<RequestInfo_Couriers> courier_requests { get; set; }

        public void RestaurantFill(List<RequestInfo_Restaurants> listOut)
        {
            restaurant_requests = listOut;
        }

        public void CourierFill(List<RequestInfo_Couriers> listOut)
        {
            courier_requests = listOut;
        }

    }
}
