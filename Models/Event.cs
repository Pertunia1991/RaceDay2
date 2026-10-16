
using RaceDay2.API.Models;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Models;

public class Event
{
    public int EventID { get; set; }

    public int OrganiserID { get; set; }
    public int EventTypeID { get; set; }

    [Required, MaxLength(100)]
    public string EventName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public DateTime EventDate { get; set; }

    [Required, MaxLength(200)]
    public string Location { get; set; } = string.Empty;

    public decimal Distance { get; set; }

    [MaxLength(500)]
    public string? BannerImageURL { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public User? Organiser { get; set; }
    public EventType? EventType { get; set; }

    public ICollection<Category> Categories { get; set; }
        = new List<Category>();

    public ICollection<Enrolment> Enrolments { get; set; }
        = new List<Enrolment>();
}
