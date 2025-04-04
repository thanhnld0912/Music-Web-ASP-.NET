using Microsoft.EntityFrameworkCore;
using MusicWeb.Models;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
    public ApplicationDbContext() { }

    public DbSet<User> Users { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<Song> Songs { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Like> Likes { get; set; }
    public DbSet<Dislike> Dislikes { get; set; }
    public DbSet<Follow> Follows { get; set; } // Added Follow DbSet

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("Your_Connection_String");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Thiết lập quan hệ User - Post
        modelBuilder.Entity<Post>()
            .HasOne(p => p.User)
            .WithMany(u => u.Posts)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Quan hệ giữa User và Like
        modelBuilder.Entity<Like>()
            .HasOne(l => l.User)
            .WithMany(u => u.Likes)
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Like>()
            .HasOne(l => l.Post)
            .WithMany(p => p.Likes)
            .HasForeignKey(l => l.PostId)
            .OnDelete(DeleteBehavior.NoAction);

        // Quan hệ giữa User và Dislike
        modelBuilder.Entity<Dislike>()
            .HasOne(d => d.User)
            .WithMany()
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Dislike>()
            .HasOne(d => d.Post)
            .WithMany()
            .HasForeignKey(d => d.PostId)
            .OnDelete(DeleteBehavior.NoAction);

        // Quan hệ giữa Comment và Post
        modelBuilder.Entity<Comment>()
            .HasOne(c => c.Post)
            .WithMany(p => p.Comments)
            .HasForeignKey(c => c.PostId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Like>()
            .HasOne(l => l.Post)
            .WithMany(p => p.Likes)
            .HasForeignKey(l => l.PostId)
            .OnDelete(DeleteBehavior.Restrict);

        // Cấu hình quan hệ giữa Like và Song
        modelBuilder.Entity<Like>()
            .HasOne(l => l.Song)
            .WithMany(s => s.Likes)
            .HasForeignKey(l => l.SongId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Dislike>()
            .HasOne(l => l.Post)
            .WithMany(p => p.Dislikes)
            .HasForeignKey(l => l.PostId)
            .OnDelete(DeleteBehavior.Restrict);

        // Cấu hình quan hệ giữa Like và Song
        modelBuilder.Entity<Dislike>()
            .HasOne(l => l.Song)
            .WithMany(s => s.Dislikes)
            .HasForeignKey(l => l.SongId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure Follow relationships
        modelBuilder.Entity<Follow>()
            .HasOne(f => f.Follower)
            .WithMany(u => u.Following)
            .HasForeignKey(f => f.FollowerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Follow>()
            .HasOne(f => f.Following)
            .WithMany(u => u.Followers)
            .HasForeignKey(f => f.FollowingId)
            .OnDelete(DeleteBehavior.Restrict);
    }
    public static void Seed(ApplicationDbContext context)
    {
        // Kiểm tra nếu cơ sở dữ liệu không có người dùng nào
        if (!context.Users.Any())
        {
            var adminUser = new User
            {
                Username = "admin",
                Email = "admin@example.com",
                Password = "Admin@123", // Bạn có thể mã hóa mật khẩu nếu cần
                Role = "Admin",  // Đặt vai trò là Admin
                CreatedAt = DateTime.Now,
                Bio = "Administrator of the MusicWeb platform"
            };

            context.Users.Add(adminUser);
            context.SaveChanges();
        }
    }
}