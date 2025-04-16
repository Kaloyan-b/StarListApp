using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace StarListApp.Models
{
    public class Setlist
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public List<Song> Songs { get; set; } = new();

        [NotMapped]
        public TimeSpan TotalDuration => TimeSpan.FromSeconds(Songs.Sum(s => s.Duration.TotalSeconds));

        [Required]
        public string UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public StarListUser User { get; set; }
    }
}
