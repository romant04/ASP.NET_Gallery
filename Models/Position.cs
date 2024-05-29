using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Gallery.Models
{
    public class Position
    {
        [Key]
        public int Id { get; set; }

        public int? row { get; set; }
        public int? column { get; set; }
        public int? order { get; set; }
        public Guid UserId { get; set; }
    }
}
