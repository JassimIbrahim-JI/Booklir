using System.ComponentModel.DataAnnotations;

namespace Booklir.enums
{
    public enum BookingStatus
    {
        [Display(Name = "Pending")]
        Pending,

        [Display(Name = "Confirmed")]
        Confirmed,

        [Display(Name = "Cancelled")]
        Cancelled,
    }
}
