
using System.ComponentModel.DataAnnotations;

namespace RaceDay.MVC.Models
{
    public class CategoryViewModel
    {
        public int CategoryID { get; set; }

        [Required(ErrorMessage = "Please select an event.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid event.")]
        public int EventID { get; set; }

        [Required(ErrorMessage = "Category name is required.")]
        [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters.")]
        public string CategoryName { get; set; } = string.Empty;

        [Range(1, 120, ErrorMessage = "Minimum age must be between 1 and 120.")]
        public int? MinimumAge { get; set; }

        [Range(1, 120, ErrorMessage = "Maximum age must be between 1 and 120.")]
        public int? MaximumAge { get; set; }

        [Range(0.01, 1000, ErrorMessage = "Distance must be greater than zero.")]
        public decimal Distance { get; set; }
    }
}
