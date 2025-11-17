using Microsoft.AspNetCore.Identity;  // Добавлено для IdentityRole
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PollApp.Models;

namespace PollApp.Data
{
    public class AppDbContext : IdentityDbContext<AppUser, IdentityRole, string>  // Исправлено: добавлены типы
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Poll> Polls { get; set; }
        public DbSet<Option> Options { get; set; }
        public DbSet<Vote> Votes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Option>()
                .HasOne(o => o.Poll)
                .WithMany(p => p.Options)
                .HasForeignKey(o => o.PollId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Vote>()
                .HasOne(v => v.Option)
                .WithMany(o => o.Votes)
                .HasForeignKey(v => v.OptionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Vote>()
                .HasOne(v => v.User)
                .WithMany()
                           .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique constraint for one vote per user per poll (enforced in service if not here)
            modelBuilder.Entity<Vote>()
                .HasIndex(v => new { v.UserId, v.OptionId })
                .IsUnique(false);  // Adjusted if needed
        }
    }
}