namespace BillionSongs.Data {
using System.ComponentModel.DataAnnotations;
public class SongVote {
    [Required]
    public uint SongID { get; set; }
    [Required]
    public string UserID { get; set; }
    [Required]
    public bool Upvote { get; set; }
}
}
