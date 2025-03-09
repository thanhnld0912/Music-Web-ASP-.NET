using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace MusicWeb.Models
{
    public class Song
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [ForeignKey("User")]
        public int ArtistId { get; set; }

        public string? FileUrl { get; set; }
        public string? CoverImage { get; set; }
        public string? Lyrics { get; set; }
        public DateTime UploadDate { get; set; } = DateTime.Now;
        public string Status { get; set; } // Public, Private

        public virtual User User { get; set; }
    }
}
