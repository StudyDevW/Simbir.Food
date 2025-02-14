using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.Model.DBO
{
    public class Restaurant
    {
        [Key]
        public Guid id { get; set; }

        public string name { get; set; }

        public string? Img { get; set; }

        public string description { get; set; }

        public string phone_number { get; set; }

        public string address { get; set; }

        public string status { get; set; }

        public string login { get; set; }

        public string password { get; set; }
    }
}
