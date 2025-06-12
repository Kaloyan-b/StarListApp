using System.ComponentModel.DataAnnotations;

namespace StarListApp.ViewModels
{
    public class AddSongViewModel
    {
        public int SetlistId { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        [Display(Name = "Duration (hh:mm:ss)")]
        public string Duration { get; set; }

        public int BPM { get; set; }
        public int Order { get; set; }

        [Required]
        public string Key { get; set; }
    }



}
