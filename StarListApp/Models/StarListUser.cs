using Microsoft.AspNetCore.Identity;

namespace StarListApp.Models
{
    public class StarListUser : IdentityUser
    {
        public ICollection<Setlist> Setlists { get; set; }
        public ICollection<Song> Songs { get; set; }
    }
}
