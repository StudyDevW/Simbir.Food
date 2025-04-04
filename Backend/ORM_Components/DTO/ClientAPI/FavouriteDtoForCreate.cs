namespace ORM_Components.DTO.ClientAPI
{
    public class FavouriteDtoForCreateAndDelete
    {
        public Guid UserId { get; set; }
        public Guid RestaurantId { get; set; }
    }
}
