﻿    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using MusicWeb.Data;
    using MusicWeb.Models;

    namespace MusicWeb.Controllers
    {
        public class UserController : Controller
        {
            private readonly ApplicationDbContext _context;

            public UserController(ApplicationDbContext context)
            {
                _context = context;
            }


        // This method now handles viewing any user's profile
        public async Task<IActionResult> Profile(int id)
        {
            // Kiểm tra xem người dùng có đang đăng nhập và có quyền xem hồ sơ của người khác không
            int currentUserId = 0;
            if (int.TryParse(HttpContext.Session.GetString("UserId"), out currentUserId) && currentUserId == id)
            {
                // Nếu người dùng đang xem chính hồ sơ của mình, chuyển hướng tới trang profile của tài khoản
                return RedirectToAction("Profile", "Account");
            }

            // Lấy thông tin người dùng từ cơ sở dữ liệu
            var user = await _context.Users
                .Include(u => u.Posts)
                    .ThenInclude(p => p.Songs)
                .FirstOrDefaultAsync(u => u.Id == id);

            // Nếu không tìm thấy người dùng, trả về NotFound
            if (user == null)
            {
                return NotFound();
            }

            // Kiểm tra xem người dùng hiện tại có đang theo dõi người dùng này không
            bool isFollowing = false;
            if (currentUserId > 0)
            {
                isFollowing = await _context.Follows
                    .AnyAsync(f => f.FollowerId == currentUserId && f.FollowingId == id);
            }

            ViewBag.IsFollowing = isFollowing;
            return View(user);
        }


        // Follow/Unfollow methods remain the same
        [HttpPost]
            public async Task<IActionResult> Follow(int followingId)
            {
                // Existing code remains unchanged
                // Check if user is authenticated
                if (!int.TryParse(HttpContext.Session.GetString("UserId"), out int currentUserId))
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }
                // Check if users exist
                var currentUser = await _context.Users.FindAsync(currentUserId);
                var userToFollow = await _context.Users.FindAsync(followingId);
                if (currentUser == null || userToFollow == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }
                // Prevent following oneself
                if (currentUserId == followingId)
                {
                    return Json(new { success = false, message = "Cannot follow yourself" });
                }
                // Check if already following
                var existingFollow = await _context.Follows
                    .FirstOrDefaultAsync(f => f.FollowerId == currentUserId && f.FollowingId == followingId);
                if (existingFollow != null)
                {
                    return Json(new { success = false, message = "Already following this user" });
                }
                // Create new follow relationship
                var follow = new Follow
                {
                    FollowerId = currentUserId,
                    FollowingId = followingId,
                    FollowedAt = DateTime.Now
                };
                _context.Follows.Add(follow);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }

            [HttpPost]
            public async Task<IActionResult> Unfollow(int followingId)
            {
                // Existing code remains unchanged
                // Check if user is authenticated
                if (!int.TryParse(HttpContext.Session.GetString("UserId"), out int currentUserId))
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }
                // Find the follow relationship
                var follow = await _context.Follows
                    .FirstOrDefaultAsync(f => f.FollowerId == currentUserId && f.FollowingId == followingId);
                if (follow == null)
                {
                    return Json(new { success = false, message = "Follow relationship not found" });
                }
                // Remove follow relationship
                _context.Follows.Remove(follow);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
        }
    }