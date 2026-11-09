using System.ComponentModel.DataAnnotations;

namespace RaceDay.MVC.Models
{
    public class EventTypeViewModel
    {
        public int EventTypeID { get; set; }

        [Required(ErrorMessage = "Event type name is required.")]
        [StringLength(100)]
        public string TypeName { get; set; } = string.Empty;
    }
}