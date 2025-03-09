using Microsoft.EntityFrameworkCore;
using MusicWeb.Models;
using System.Collections.Generic;

namespace MusicWeb.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // Constructor mặc định để hỗ trợ migration khi chạy từ lệnh CLI
        public ApplicationDbContext() { }

        public DbSet<User> Users { get; set; }
        public DbSet<Song> Songs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=MusicWeb;Trusted_Connection=True;MultipleActiveResultSets=true");
            }
        }
    }
}
