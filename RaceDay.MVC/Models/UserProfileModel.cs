
using System.ComponentModel.DataAnnotations;

namespace RaceDay.MVC.Models
{
    public class UserProfileViewModel
    {
        public int UserID { get; set; }

        [Required(ErrorMessage = "First name is required.")]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        public string Role { get; set; } = string.Empty;

        public string? ProfilePictureURL { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
