using System.ComponentModel.DataAnnotations;

namespace Booklir.ViewModels.Admin
{
    public class BulkActionModel
    {
        [Required]
        public string Action { get; set; }

        [Required]
        public List<int> Ids { get; set; }
    }
}
