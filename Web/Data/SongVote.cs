namespace BillionSongs.Data {
using System.ComponentModel.DataAnnotations;
public class SongVote {
    [Required]
    public uint SongID { get; set; }
    public Song Song{ get; set; }
    [Required]
    public string UserID { get; set; }
    [Required]
    public bool Upvote { get; set; }
}
}
