using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;

public class PostController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _env;

    public PostController(ApplicationDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    [HttpPost]
    public async Task<IActionResult> LikePost(int id, [FromBody] int userId, [FromQuery] string type = "post")
    {
        try
        {
            // Xác định loại (post hoặc song)
            bool isPost = type.ToLower() == "post";

            // Xóa dislike nếu có
            var existingDislike = await _context.Dislikes
                .FirstOrDefaultAsync(d =>
                    (isPost ? d.PostId == id : d.SongId == id) &&
                    d.UserId == userId);

            if (existingDislike != null)
            {
                _context.Dislikes.Remove(existingDislike);
            }

            // Kiểm tra like hiện có
            var existingLike = await _context.Likes
                .FirstOrDefaultAsync(l =>
                    (isPost ? l.PostId == id : l.SongId == id) &&
                    l.UserId == userId);

            if (existingLike != null)
            {
                _context.Likes.Remove(existingLike);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Like removed successfully" });
            }
            else
            {
                // Tạo like mới
                var like = new Like
                {
                    UserId = userId,
                    PostId = isPost ? id : null,
                    SongId = isPost ? null : id,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Likes.Add(like);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Liked successfully" });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred", error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> DislikePost(int id, [FromBody] int userId)
    {
        try
        {
            // Kiểm tra xem người dùng đã like bài viết này chưa
            var existingLike = await _context.Likes
                .FirstOrDefaultAsync(l => l.PostId == id && l.UserId == userId);

            if (existingLike != null)
            {
                // Nếu đã like, xóa like
                _context.Likes.Remove(existingLike);
            }

            // Kiểm tra xem người dùng đã dislike bài viết này chưa
            var existingDislike = await _context.Dislikes
                .FirstOrDefaultAsync(d => d.PostId == id && d.UserId == userId);

            if (existingDislike != null)
            {
                // Nếu đã dislike, xóa dislike (để hủy dislike)
                _context.Dislikes.Remove(existingDislike);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Dislike removed successfully" });
            }
            else
            {
                // Nếu chưa dislike, thêm dislike mới
                var dislike = new Dislike { UserId = userId, PostId = id, CreatedAt = DateTime.UtcNow };
                _context.Dislikes.Add(dislike);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Post disliked successfully" });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred", error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetComments(int id)
    {
        try
        {
            var comments = await _context.Comments
                .Include(c => c.User)
                .Where(c => c.PostId == id)
                .OrderBy(c => c.CreatedAt)
                .Select(c => new
                {
                    id = c.Id,
                    content = c.Content,
                    userId = c.UserId,
                    userName = c.User.Username,
                    createdAt = c.CreatedAt
                })
                .ToListAsync();

            return Json(comments);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred", error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> AddComment(int id, [FromBody] CommentInputModel comment)
    {
        try
        {
            if (comment == null || string.IsNullOrWhiteSpace(comment.Content))
            {
                return BadRequest(new { message = "Comment content cannot be empty" });
            }

            var newComment = new Comment
            {
                PostId = id,
                UserId = comment.UserId,
                Content = comment.Content,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(newComment);
            await _context.SaveChangesAsync();

            // Lấy thông tin người dùng
            var user = await _context.Users.FindAsync(comment.UserId);
            string username = user?.Username ?? "User";

            return Ok(new
            {
                id = newComment.Id,
                content = newComment.Content,
                userId = newComment.UserId,
                userName = username,
                createdAt = newComment.CreatedAt
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred", error = ex.Message });
        }
    }
    [HttpGet]
    public IActionResult GetCommentCount(int id)
    {
        var post = _context.Posts
            .Include(p => p.Comments)
            .FirstOrDefault(p => p.Id == id);

        if (post == null)
        {
            return NotFound();
        }

        int commentCount = post.Comments?.Count ?? 0;
        return Json(commentCount);
    }
}


// Model cho việc nhận comment từ client
public class CommentInputModel
{
    public string Content { get; set; }
    public int UserId { get; set; }
}