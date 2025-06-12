namespace StarListApp.ViewModels
{
    public class SetlistDetailsViewModel
    {
        public int SetlistId { get; set; }
        public List<SongItem> Songs { get; set; }

        public class SongItem
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public int BPM { get; set; }
            public string Key { get; set; }
            public string Duration { get; set; } // тук е стринг поради грешка във въвеждането
            public int Order { get; set; }
            public int SetlistId { get; set; }
        }
    }
}

