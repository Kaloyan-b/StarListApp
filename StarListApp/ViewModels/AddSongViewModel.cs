using System.ComponentModel.DataAnnotations;

namespace StarListApp.ViewModels
{
    public class AddSongViewModel
    {
        public int SetlistId { get; set; }

        [Required]
        public string Title { get; set; }

        [Display(Name = "Duration (e.g. 03:30)")]
        public TimeSpan Duration { get; set; }
    }

}
