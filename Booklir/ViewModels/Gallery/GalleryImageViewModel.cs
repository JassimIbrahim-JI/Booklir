namespace Booklir.ViewModels.Gallery
{
    public class GalleryImageViewModel
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; }
        public bool IsPrimary { get; set; }

        // This ViewModel is used for displaying images in the gallery
        // It can be extended with additional properties as needed
    }
}
