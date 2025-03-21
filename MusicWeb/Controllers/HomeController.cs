using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicWeb.Data;
using MusicWeb.Models;
using System.Linq;
using System.Threading.Tasks;

namespace MusicWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> NewFeed()
        {
            var posts = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Songs)
                .Include(p => p.Likes)
                .Include(p => p.Dislikes)
                .Include(p => p.Comments)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            // Get current user ID
            if (int.TryParse(HttpContext.Session.GetString("UserId"), out int currentUserId))
            {
                // For each post, check if the current user has liked or disliked it
                foreach (var post in posts)
                {
                    ViewData[$"UserLiked_{post.Id}"] = post.Likes?.Any(l => l.UserId == currentUserId) ?? false;
                    ViewData[$"UserDisliked_{post.Id}"] = post.Dislikes?.Any(d => d.UserId == currentUserId) ?? false;
                }
            }

            return View(posts);
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Loader()
        {
            return View();
        }

        public async Task<IActionResult> ProfileUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Get user's posts with all related data
            var userPosts = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Songs)
                .Include(p => p.Likes)
                .Include(p => p.Dislikes)
                .Include(p => p.Comments)
                .Where(p => p.UserId == id)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            // Get user's songs that aren't attached to posts
            var userSongs = await _context.Songs
                .Where(s => s.ArtistId == id)  
                .OrderByDescending(s => s.UploadDate)
                .ToListAsync();

            // Count followers and following
            var followerCount = await _context.Follows
                .CountAsync(f => f.FollowingId == id);
            var followingCount = await _context.Follows
                .CountAsync(f => f.FollowerId == id);

            ViewBag.FollowerCount = followerCount;
            ViewBag.FollowingCount = followingCount;

            // Get current user ID from session
            var currentUserId = HttpContext.Session.GetString("UserId");

            // Check if current user is viewing their own profile
            bool isOwnProfile = false;
            if (!string.IsNullOrEmpty(currentUserId) && int.Parse(currentUserId) == id)
            {
                isOwnProfile = true;
            }

            // Check if current user is following this profile
            if (!string.IsNullOrEmpty(currentUserId))
            {
                var isFollowing = await _context.Follows
                    .AnyAsync(f => f.FollowerId == int.Parse(currentUserId) && f.FollowingId == id);
                ViewBag.IsFollowing = isFollowing;

                // For each post, check if the current user has liked or disliked it
                foreach (var post in userPosts)
                {
                    ViewData[$"UserLiked_{post.Id}"] = post.Likes?.Any(l => l.UserId == int.Parse(currentUserId)) ?? false;
                    ViewData[$"UserDisliked_{post.Id}"] = post.Dislikes?.Any(d => d.UserId == int.Parse(currentUserId)) ?? false;
                }
            }

            // Pass the user's posts and songs to the view
            ViewBag.UserPosts = userPosts;
            ViewBag.UserSongs = userSongs;
            ViewBag.IsOwnProfile = isOwnProfile;

            return View(user);
        }
    }
}