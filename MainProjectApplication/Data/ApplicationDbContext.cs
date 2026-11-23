using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PUPApp.Models;

namespace PUPApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<ArtistProfile> ArtistProfiles { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Album> Albums { get; set; }
        public DbSet<Track> Tracks { get; set; }
        public DbSet<Playlist> Playlists { get; set; }
        public DbSet<PlaylistTrack> PlaylistTracks { get; set; }
        public DbSet<TrackLike> TrackLikes { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // One-to-one: ApplicationUser <-> ArtistProfile
            builder.Entity<ArtistProfile>()
                .HasOne(a => a.User)
                .WithOne(u => u.ArtistProfile)
                .HasForeignKey<ArtistProfile>(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Store enums as readable text instead of numbers
            builder.Entity<ArtistProfile>().Property(a => a.Status).HasConversion<string>().HasMaxLength(20);
            builder.Entity<Track>().Property(t => t.ReleaseType).HasConversion<string>().HasMaxLength(20);
            builder.Entity<Track>().Property(t => t.Visibility).HasConversion<string>().HasMaxLength(20);

            // Many-to-many: Track <-> Genre
            builder.Entity<Track>()
                .HasMany(t => t.Genres)
                .WithMany(g => g.Tracks);

            // Composite keys for the other join tables
            builder.Entity<PlaylistTrack>().HasKey(pt => new { pt.PlaylistId, pt.TrackId });
            builder.Entity<TrackLike>().HasKey(l => new { l.UserId, l.TrackId });

            // SQL Server rejects "multiple cascade paths", so these use Restrict.
            // Delete a track's likes and playlist entries in the controller first.
            builder.Entity<PlaylistTrack>()
                .HasOne(pt => pt.Track).WithMany(t => t.PlaylistTracks)
                .HasForeignKey(pt => pt.TrackId).OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TrackLike>()
                .HasOne(l => l.Track).WithMany(t => t.Likes)
                .HasForeignKey(l => l.TrackId).OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Genre>().HasData(GenreCatalog.Seed());
        }
    }
}
