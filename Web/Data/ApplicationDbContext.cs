namespace BillionSongs.Data {
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;

    public class ApplicationDbContext : IdentityDbContext {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) {
        }

        public DbSet<Song> Songs { get; set; }
    }
}
