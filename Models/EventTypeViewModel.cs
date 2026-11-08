
using System.ComponentModel.DataAnnotations;

namespace RaceDay2.API.Models
{
    public class EventType
    {
        public int EventTypeID { get; set; }

        [Required]
        [MaxLength(100)]
        public string TypeName { get; set; } = string.Empty;

        public ICollection<Event> Events { get; set; }
            = new List<Event>();
    }
}
