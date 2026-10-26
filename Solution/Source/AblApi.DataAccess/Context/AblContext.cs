using AblApi.DataAccess.Models;
using AblApi.DataAccess.Models.GTNH;
using AblApi.DataAccess.Models.Spotify;
using AblApi.DataAccess.Models.Willhaben;
using Microsoft.EntityFrameworkCore;

namespace AblApi.DataAccess.Context;

public class AblContext(DbContextOptions<AblContext> options) : DbContext(options)
{
    public virtual DbSet<SchemaVersion> SchemaVersions { get; set; }

    public virtual DbSet<ApiUser> ApiUsers { get; set; }

    public virtual DbSet<ApiAccessRoleGrant> ApiAccessRoleGrants { get; set; }

    public virtual DbSet<StableVersion> GTNHStableVersions { get; set; }

    public virtual DbSet<DailyVersion> GTNHDailyVersions { get; set; }

    public virtual DbSet<WillhabenConfig> WillhabenConfigs { get; set; }

    public virtual DbSet<WillhabenSeenListing> WillhabenSeenListings { get; set; }

    public virtual DbSet<SpotifyUser> SpotifyUsers { get; set; }

    public virtual DbSet<SpotifyPlaylistCache> SpotifyPlaylistCaches { get; set; }

    public virtual DbSet<SpotifyPlaylistCacheTrack> SpotifyPlaylistCacheTracks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SchemaVersion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_SchemaVersions_Id");

            entity.Property(e => e.AppliedAt).HasColumnType("datetime");
            entity.Property(e => e.ScriptName).HasMaxLength(255);
        });

        modelBuilder.Entity<ApiUser>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Token).HasMaxLength(255);
        });

        modelBuilder.Entity<ApiAccessRoleGrant>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.GrantedAt).HasColumnType("datetime");

            entity.HasOne(d => d.User).WithMany(p => p.Roles)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientCascade)
                .HasConstraintName("FK_ApiAccessRoleGrants_ApiUsers");
        });

        modelBuilder.Entity<StableVersion>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Version).HasMaxLength(32);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<DailyVersion>(entity =>
        {
            entity.HasKey(e => e.RunNumber);

            entity.Property(e => e.Version).HasMaxLength(32);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.RunUrl).HasMaxLength(255);
            entity.Property(e => e.RunHtmlUrl).HasMaxLength(255);
            entity.Property(e => e.ClientDownloadUrl).HasMaxLength(255);
            entity.Property(e => e.ClientDownloadUrlJava8).HasMaxLength(255);
            entity.Property(e => e.ServerDownloadUrl).HasMaxLength(255);
            entity.Property(e => e.ServerDownloadUrlJava8).HasMaxLength(255);
        });

        modelBuilder.Entity<WillhabenConfig>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.Name).IsUnique();
        });

        modelBuilder.Entity<WillhabenSeenListing>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.Url).IsUnique();
        });

        modelBuilder.Entity<SpotifyUser>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.UserIdentifier).HasMaxLength(255);
            entity.Property(e => e.SpotifyId).HasMaxLength(255);
            entity.Property(e => e.LinkedAt).HasColumnType("datetime");

            entity.HasIndex(e => e.UserIdentifier).IsUnique();
            entity.HasIndex(e => e.SpotifyId).IsUnique();
        });

        modelBuilder.Entity<SpotifyPlaylistCache>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.PlaylistId).HasMaxLength(255);
            entity.Property(e => e.SnapshotId).HasMaxLength(255);

            entity.HasIndex(e => new { e.SpotifyUserId, e.PlaylistId }).IsUnique();

            entity.HasOne(d => d.SpotifyUser).WithMany(p => p.Playlists)
                .HasForeignKey(d => d.SpotifyUserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_SpotifyPlaylistCaches_SpotifyUsers");
        });

        modelBuilder.Entity<SpotifyPlaylistCacheTrack>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.TrackId).HasMaxLength(255);
            entity.Property(e => e.AddedAt).HasColumnType("datetime");

            entity.HasIndex(e => new { e.PlaylistCacheId, e.TrackId }).IsUnique();

            entity.HasOne(d => d.PlaylistCache).WithMany(p => p.Tracks)
                .HasForeignKey(d => d.PlaylistCacheId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_SpotifyPlaylistCacheTracks_SpotifyPlaylistCaches");
        });
    }
}
