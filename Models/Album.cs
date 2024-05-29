using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Gallery.Models
{
    public class Album
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }
        public IdentityUser Uploader { get; set; }
        public bool IsPublic { get; set; }
        public bool IsDefault { get; set; }
        public ICollection<StoredFile>? Files { get; set; }
        public Guid? CoverImageId { get; set; }
    }
}
