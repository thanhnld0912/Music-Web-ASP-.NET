﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicWeb.Models
{
    public class PlaylistSong
    {
        [Key]
        public int Id { get; set; }

        public int PlaylistId { get; set; }
        public int SongId { get; set; }

        [ForeignKey("PlaylistId")]
        public virtual Playlist Playlist { get; set; }

        [ForeignKey("SongId")]
        public virtual Song Song { get; set; }
    }
}
