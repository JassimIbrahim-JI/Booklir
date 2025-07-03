using Booklir.Models;
using Booklir.Queries;

namespace Booklir.ViewModels.User
{
    public class UserListViewModel
    {
        public IEnumerable<UserViewModel> Items { get; set; }
        public UserQueryParameters Parameters { get; set; }
        public int TotalCount { get; set; }
    }
}
