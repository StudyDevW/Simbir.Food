using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.Model.DBO
{
    public class Photo
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }
    }
}
