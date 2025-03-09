using System.ComponentModel.DataAnnotations;

namespace MusicWeb.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string Username { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }

        public string Role { get; set; } // Listener, Uploader, VIP

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
