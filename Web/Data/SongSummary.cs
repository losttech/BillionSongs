namespace BillionSongs.Data {
    public struct SongSummary {
        public Song Song { get; set; }
        public int Upvotes;
        public int Downvotes;

        public int VoteSum => this.Upvotes - this.Downvotes;

        public SongSummary Clone()
            => new SongSummary {
                Song = this.Song,
                Upvotes = this.Upvotes,
                Downvotes = this.Downvotes,
            };
    }
}