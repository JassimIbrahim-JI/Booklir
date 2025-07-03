using Booklir.enums;
using System.ComponentModel.DataAnnotations;

namespace Booklir.ViewModels.User
{
    public class ContactViewModel
    {
        [Required(ErrorMessage = "Your name is required")]
        [Display(Name = "Full Name")]
        [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Subject is required")]
        [StringLength(100, ErrorMessage = "Subject cannot be longer than 100 characters")]
        public string Subject { get; set; }

        [Required(ErrorMessage = "Message is required")]
        [DataType(DataType.MultilineText)]
        [StringLength(2000, ErrorMessage = "Message cannot be longer than 2000 characters")]
        public string Message { get; set; }

        [Display(Name = "Phone Number")]
        [Phone(ErrorMessage = "Invalid phone number")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Preferred Contact Method")]
        public ContactMethod PreferredMethod { get; set; } = ContactMethod.Email;
    }
}
