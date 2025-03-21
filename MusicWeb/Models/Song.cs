using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicWeb.Models
{
    public class Song
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [ForeignKey("User")]
        public int ArtistId { get; set; }

        [ForeignKey("Post")]
        public int? PostId { get; set; } // Cho phép NULL

        public string? FileUrl { get; set; }
        public string? CoverImage { get; set; }
        public string? Lyrics { get; set; }
        public string? Genre { get; set; }
        public string? Era { get; set; }
        public string? Type { get; set; } // Cover or Original
        public DateTime UploadDate { get; set; } = DateTime.Now;
        public string Status { get; set; } // Public, Private

        public virtual User User { get; set; }
        public virtual Post? Post { get; set; }
        public virtual ICollection<Like> Likes { get; set; }
        public virtual ICollection<Dislike> Dislikes { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
    }
}
