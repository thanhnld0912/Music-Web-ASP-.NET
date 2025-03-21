using Microsoft.AspNetCore.Mvc;
using MusicWeb.Data;
using MusicWeb.Models;

namespace MusicWeb.Controllers
{
    public class SongController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public SongController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // Hiển thị trang upload
        public IActionResult UploadPage()
        {
            return View();
        }

        // Xử lý upload bài hát kèm bài viết
        [HttpPost]
        public async Task<IActionResult> Upload(
            IFormFile file,       
            string title,           
            int artistId,           
            string content,       
            string genre,         
            string era,           
            string type,            
            string description)    
        {
            try
            {
            

                // Kiểm tra file
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { success = false, message = "Vui lòng chọn một tệp để tải lên." });
                }

                // Kiểm tra định dạng file
                var allowedExtensions = new[] { ".mp3", ".m4a" };
                var fileExtension = Path.GetExtension(file.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest(new { success = false, message = "Chỉ hỗ trợ các tệp MP3 hoặc M4A." });
                }

                // Kiểm tra thông tin bài hát cơ bản
                if (string.IsNullOrEmpty(title) || artistId <= 0)
                {
                    return BadRequest(new { success = false, message = "Thiếu tiêu đề hoặc ID nghệ sĩ không hợp lệ." });
                }

              

                // Tạo thư mục upload nếu chưa tồn tại
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Tạo tên file duy nhất để tránh trùng lặp
                var uniqueFileName = $"{Path.GetFileNameWithoutExtension(file.FileName)}_{Guid.NewGuid().ToString("N")}{fileExtension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Lưu file vào server
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

            

                // Tạo nội dung mặc định nếu không có
                var postContent = !string.IsNullOrEmpty(description)
                    ? description
                    : !string.IsNullOrEmpty(content)
                        ? content
                        : $"Bài hát mới: {title}";

                var post = new Post
                {
                    UserId = artistId,
                    Content = postContent,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.Set<Post>().Add(post);
                await _context.SaveChangesAsync(); // Lưu để lấy ID

      

                var song = new Song
                {
                    Title = title,
                    ArtistId = artistId,
                    PostId = post.Id,                      // Liên kết với bài viết
                    FileUrl = "/uploads/" + uniqueFileName,
                    Genre = genre,
                    Era = era,
                    Type = type,
                    UploadDate = DateTime.Now,
                    Status = "Public"
                };

                _context.Set<Song>().Add(song);
                await _context.SaveChangesAsync();

      

                return Ok(new
                {
                    success = true,
                    message = "Tải lên thành công!",
                    fileUrl = song.FileUrl,
                    postId = post.Id,
                    songId = song.Id
                });
            }
            catch (Exception ex)
            {
                // Xử lý lỗi và trả về thông báo
                return StatusCode(500, new { success = false, message = "Lỗi server: " + ex.Message });
            }
        }
    }
}