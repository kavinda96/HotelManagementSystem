using Microsoft.AspNetCore.Identity;

namespace RazorPagesMovie.Models
{
    public class ApplicationUser : IdentityUser
    {

        public int SecondaryUserId { get; set; }
        public bool isAdmin { get; set; } = false;
        public bool isSuperAdmin { get; set; }  =false;
    }
}
