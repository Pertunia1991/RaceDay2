
using System.ComponentModel.DataAnnotations;

namespace RaceDay.MVC.Models
{
    public class EnrolmentRequest
    {
        [Required]
        public int EventID { get; set; }

        [Required(ErrorMessage = "Please select a category.")]
        public int CategoryID { get; set; }
    }
}
