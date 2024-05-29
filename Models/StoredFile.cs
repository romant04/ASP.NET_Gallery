using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gallery.Models
{
    public class StoredFile
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }
        [ForeignKey("UploaderId")]
        public IdentityUser Uploader { get; set; }
        [Required]
        public string UploaderId { get; set; }
        [Required]
        public DateTime UploadedAt { get; set; }
        [Required]
        public string OriginalName { get; set; }
        [Required]
        public string ContentType { get; set; }
        public bool IsPublic { get; set; }
        public DateTime DateTaken { get; set; }
        public ICollection<Thumbnail> Thumbnails { get; set; }
        public  ICollection<Album> Album { get; set; }
        public List<Position> Position { get; set; }
        public List<GalleryOrder>? Order { get; set; }
    }
}
