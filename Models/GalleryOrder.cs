namespace Gallery.Models
{
    public class GalleryOrder
    {
        public int Id { get; set; }
        public int GalleryId { get; set; }
        public Guid StoredFileId { get; set; }
        public int Order { get; set; }
    }
}
