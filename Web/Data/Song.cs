namespace BillionSongs.Data {
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    public class Song {
        [Key]
        public uint ID { get; set; }
        [MaxLength(129)]
        public string Title { get; set; }
        [MaxLength(4096)]
        public string Lyrics { get; set; }
        public DateTimeOffset Generated { get; set; }
        public string GeneratorError { get; set; }
        public List<SongVote> Votes { get; set; }
    }
}
