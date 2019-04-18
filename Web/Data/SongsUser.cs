namespace BillionSongs.Data {
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
public class SongsUser: IdentityUser {
    [ForeignKey(nameof(SongVote.UserID))]
    public List<SongVote> SongVotes { get; set; }
}
}
