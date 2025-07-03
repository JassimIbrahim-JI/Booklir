namespace Booklir.ViewModels.User
{
    public class UserViewModel
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
        public string ProfileImageUrl { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public List<string> Roles { get; set; } = new();
    }
}
