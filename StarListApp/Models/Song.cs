using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace StarListApp.Models
{
    public class Song
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Artist { get; set; }

        [Required]
        public TimeSpan Duration { get; set; }

        public int BPM { get; set; }

        [Required]
        public string Key { get; set; }

        [Required]
        public int SetlistId { get; set; }

        [ForeignKey(nameof(SetlistId))]
        public Setlist Setlist { get; set; }

        [Required]
        public string UserId { get; set; }

        public int Order { get; set; }

        [ForeignKey(nameof(UserId))]
        public StarListUser User { get; set; }

    }
}
