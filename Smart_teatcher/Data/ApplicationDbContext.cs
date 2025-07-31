using Microsoft.EntityFrameworkCore;
using Smart_teatcher.Models;
using System;

namespace Smart_teatcher.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Word> Words { get; set; }
        public DbSet<Sentence> Sentences { get; set; }
        public DbSet<MathExample> MathExample { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserPermissions> UserPermissions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Word>().Property(w => w.Level).HasDefaultValue(Level.Beginner);
            modelBuilder.Entity<Sentence>().Property(s => s.Level).HasDefaultValue(Level.Beginner);
            modelBuilder.Entity<MathExample>().Property(m => m.Level).HasDefaultValue(Level.Beginner);
            modelBuilder.Entity<MathExample>().Property(m => m.ArithmeticOperations).HasDefaultValue(ArithmeticOperations.addition);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Creator)
                .WithMany()
                .HasForeignKey(u => u.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict); 

            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasDefaultValue("Admin");
        }
    }
}
