namespace BillionSongs.Data {
    using System;
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;

    public class ApplicationDbContext : IdentityDbContext<SongsUser> {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) {
        }

        public DbSet<Song> Songs { get; set; }
        public DbSet<SongVote> Votes { get; set; }

        protected override void OnModelCreating(ModelBuilder builder) {
            base.OnModelCreating(builder);

            builder.Entity<SongVote>()
                .HasKey(vote => new { vote.SongID, vote.UserID });
        }
    }
}
