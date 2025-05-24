namespace StarListApp.ViewModels
{
    public class SetlistDetailsViewModel
    {
        public int SetlistId { get; set; }
        public string Name { get; set; }
        public List<SongItem> Songs { get; set; }

        public class SongItem
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public TimeSpan Duration { get; set; }
            public int Order { get; set; }
        }
    }
}

