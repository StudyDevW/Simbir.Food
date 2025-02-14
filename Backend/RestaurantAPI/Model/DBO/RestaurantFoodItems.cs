using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.Model.DBO
{
    public class RestaurantFoodItems
    {
        [Key]
        public int id { get; set; }
        public int restaraunt_id { get; set; }
        public string name { get; set; }
        public string price { get; set; }
        public string? Img { get; set; }
        public string weight { get; set; }
        public string calories { get; set; }
    }
}
