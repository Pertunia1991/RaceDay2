
using System.ComponentModel.DataAnnotations;

namespace RaceDay.MVC.Models
{
    public class EventViewModel
    {
        public int EventID { get; set; }

        public int OrganiserID { get; set; }

        [Required(ErrorMessage = "Please select an event type.")]
        public int EventTypeID { get; set; }

        [Required(ErrorMessage = "Event name is required.")]
        [StringLength(100)]
        public string EventName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Event date is required.")]
        public DateTime EventDate { get; set; }

        [Required(ErrorMessage = "Event location is required.")]
        [StringLength(200)]
        public string Location { get; set; } = string.Empty;

        [Range(0.01, 1000,
            ErrorMessage = "Distance must be greater than zero.")]
        public decimal Distance { get; set; }

        [StringLength(500)]
        public string? BannerImageURL { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
