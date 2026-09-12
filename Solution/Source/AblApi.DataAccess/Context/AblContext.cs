using AblApi.DataAccess.Models;
using AblApi.DataAccess.Models.GTNH;
using Microsoft.EntityFrameworkCore;

namespace AblApi.DataAccess.Context;

public class AblContext(DbContextOptions<AblContext> options) : DbContext(options)
{
    public virtual DbSet<ApiUser> ApiUsers { get; set; }

    public virtual DbSet<ApiAccessRoleGrant> ApiAccessRoleGrants { get; set; }

    public virtual DbSet<StableVersion> GTNHStableVersions { get; set; }

    public virtual DbSet<DailyVersion> GTNHDailyVersions { get; set; }

    public virtual DbSet<SchemaVersion> SchemaVersions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApiUser>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.Property(e => e.Name).HasMaxLength(64);
            entity.Property(e => e.Token).HasMaxLength(64);
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

        modelBuilder.Entity<SchemaVersion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_SchemaVersions_Id");

            entity.Property(e => e.AppliedAt).HasColumnType("datetime");
            entity.Property(e => e.ScriptName).HasMaxLength(255);
        });
    }
}
