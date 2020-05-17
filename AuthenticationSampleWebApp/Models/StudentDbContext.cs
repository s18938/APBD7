using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthenticationSampleWebApp.Models
{
    public class StudentDbContext : DbContext
    {
        public DbSet<Student> Students { get; set; }
        public StudentDbContext() { }
        public StudentDbContext(DbContextOptions options)
            : base(options)
        {
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(@"Data Source=DESKTOP-5G2FL6J\SQLEXPRESS;Initial Catalog=APBD_4;Integrated Security=True");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Student>(entity =>
            {
                entity.HasKey(e => e.Id)
                    .HasName("Student_pk");

                entity.Property(e => e.Id).HasMaxLength(100);

                entity.Property(e => e.BirthDate).HasColumnType("date");

                entity.Property(e => e.FirstName)
                            .IsRequired()
                            .HasMaxLength(100);

                entity.Property(e => e.LastName)
                            .IsRequired()
                            .HasMaxLength(100);

                
            });
        }
    }
}
