using System.ComponentModel.DataAnnotations;

namespace Gallery.InputModels
{
    public class InAlbum
    {
        [Required]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }
        public bool? IsPublic { get; set; }
    }
}
