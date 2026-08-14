using AblApi.DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace AblApi.DataAccess.Context;

public class AblContext(DbContextOptions<AblContext> options, ILogger<AblContext> logger) : DbContext(options)
{
    private readonly ILogger<AblContext> _logger = logger;

    public virtual DbSet<ApiUser> ApiUsers { get; set; }

    public virtual DbSet<ApiAccessRoleGrant> ApiAccessRoleGrants { get; set; }

    public virtual DbSet<SchemaVersion> SchemaVersions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApiUser>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.Property(e => e.Name).HasMaxLength(64);
        });

        modelBuilder.Entity<ApiAccessRoleGrant>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.GrantedAt).HasColumnType("datetime");

            entity.HasOne(d => d.User).WithMany(p => p.Roles)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ApiAccessRoleGrants_ApiUsers");
        });

        modelBuilder.Entity<SchemaVersion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_SchemaVersions_Id");

            entity.Property(e => e.AppliedAt).HasColumnType("datetime");
            entity.Property(e => e.ScriptName).HasMaxLength(255);
        });
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.LogTo((eventId, level) => true, Logger);
    }

    private void Logger(EventData data)
    {
        if (_logger is null)
        {
            return;
        }
        switch (data.LogLevel)
        {
            case LogLevel.Information: _logger.LogInformation(data.ToString()); break;
            case LogLevel.Debug: _logger.LogDebug(data.ToString()); break;
            case LogLevel.Warning: _logger.LogWarning(data.ToString()); break;
            case LogLevel.Error: _logger.LogError(data.ToString()); break;
            default: _logger.LogTrace(data.ToString()); break;
        }
    }
}
