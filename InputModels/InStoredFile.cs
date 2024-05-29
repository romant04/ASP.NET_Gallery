using Gallery.Models;
using System.ComponentModel.DataAnnotations;

namespace Gallery.InputModels
{
    public class InStoredFile
    {
        [Required]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }
        public bool? IsPublic { get; set; }

        public int? GridRow { get; set; }
        public int? GridColumn { get; set; }
        public int? RowSpan { get; set; }
        public int? ColumnSpan { get; set; }
    }
}
