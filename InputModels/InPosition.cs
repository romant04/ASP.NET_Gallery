using Microsoft.AspNetCore.Identity;

namespace Gallery.InputModels
{
    public class InPosition
    {
        public int? row { get; set; }
        public int? column { get; set; }
        public int? order { get; set; }
        public Guid UserId { get; set; }
    }
}
