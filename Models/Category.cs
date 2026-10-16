
using RaceDay2.API.Models;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Models;

public class Category
{
    public int CategoryID { get; set; }

    public int EventID { get; set; }

    [Required, MaxLength(100)]
    public string CategoryName { get; set; } = string.Empty;

    public int? MinimumAge { get; set; }
    public int? MaximumAge { get; set; }

    public decimal Distance { get; set; }

    public Event? Event { get; set; }

    public ICollection<Enrolment> Enrolments { get; set; }
        = new List<Enrolment>();
}
