using Microsoft.EntityFrameworkCore;

namespace FirstReactBackend;
public class FierstReactBackendDBContext(DbContextOptions<FierstReactBackendDBContext> options) : DbContext(options)
{
    public DbSet<UserEntity> Users { get; set; }
    public DbSet<TaskEntity> Tasks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserName).IsRequired().HasMaxLength(100);
            entity.HasMany(e => e.Tasks).WithOne(e => e.User).HasForeignKey(e => e.UserId);
        });

        modelBuilder.Entity<TaskEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Priority).IsRequired();
            entity.Property(e => e.IsDone).IsRequired();
        });
    }
}