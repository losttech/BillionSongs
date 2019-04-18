namespace BillionSongs.Pages {
    using BillionSongs.Data;

    public class SongSummary {
        public Song Song { get; set; }
        public int Upvotes { get; set; }
        public int Downvotes { get; set; }
    }
}