using LoginRegister.Entities;
using Microsoft.EntityFrameworkCore;

namespace LoginRegister;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<PasswordMigration> PasswordMigrations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // modelBuilder.Entity<PasswordMigration>()
        //     .HasOne<User>(p => p.User)
        //     .WithMany()
        //     .HasForeignKey(p => p.UserId);
    }

}