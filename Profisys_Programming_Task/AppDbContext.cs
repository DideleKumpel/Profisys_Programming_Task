using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using Microsoft.EntityFrameworkCore;
using Profisys_Programming_Task.Model;

namespace Profisys_Programming_Task
{
    class AppDbContext : DbContext
    {
        public DbSet<Documents> Documents { get; set; }
        public DbSet<DocumentItems> DocumentItems { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string connection = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            optionsBuilder.UseSqlServer(connection);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Documents>()
                .Property(d => d.Type)
                .HasConversion<string>();

            modelBuilder.Entity<DocumentItems>()
                .HasOne(d => d.Document)
                .WithMany()
                .HasForeignKey(d => d.DocumentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
