
using System.ComponentModel.DataAnnotations;

namespace RaceDay2.API.Models;

public class Result
{
    public int ResultID { get; set; }

    public int EnrolmentID { get; set; }

    public TimeSpan FinishTime { get; set; }

    public int FinishPosition { get; set; }

    public DateTime ResultDate { get; set; } = DateTime.UtcNow;

    public Enrolment? Enrolment { get; set; }
}
