using Gallery.Models;
using Microsoft.AspNetCore.Identity;

namespace Gallery.ViewModels
{
    public class StoredFileListViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public IdentityUser Uploader { get; set; }
        public string UploaderId { get; set; }
        public DateTime UploadedAt { get; set; }
        public DateTime DateTaken { get; set; }
        public string OriginalName { get; set; }
        public string ContentType { get; set; }
        public bool IsPublic { get; set; }
        public int ThumbnailCount { get; set; }
        public List<Position> Position { get; set; }
    }
}
