using System.ComponentModel.DataAnnotations.Schema;

namespace Booklir.Models
{
    public class GalleryImage
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; }
        public bool IsPrimary { get; set; }
        public int TripId { get; set; }
        public Trip Trip { get; set; }
       
      
    }
}
