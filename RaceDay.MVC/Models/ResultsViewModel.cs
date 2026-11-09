
using System.ComponentModel.DataAnnotations;

namespace RaceDay.MVC.Models
{
    public class ResultViewModel
    {
        public int ResultID { get; set; }

        [Required]
        public int EnrolmentID { get; set; }

        public int ParticipantID { get; set; }

        public int EventID { get; set; }

        public string EventName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Finish time is required.")]
        public TimeSpan FinishTime { get; set; }

        [Range(1, int.MaxValue,
            ErrorMessage = "Finish position must be greater than zero.")]
        public int FinishPosition { get; set; }

        public DateTime ResultDate { get; set; }

        public bool IsPublished { get; set; }
    }
}

