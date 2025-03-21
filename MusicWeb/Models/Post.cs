using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace MusicWeb.Models
{
    public class Post
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        [Required]
        public string Content { get; set; }

        public string? ImageUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public virtual User User { get; set; }
        public virtual ICollection<Song> Songs { get; set; } = new List<Song>();
        public virtual ICollection<Like> Likes { get; set; }
        public virtual ICollection<Dislike> Dislikes { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
    }
}
