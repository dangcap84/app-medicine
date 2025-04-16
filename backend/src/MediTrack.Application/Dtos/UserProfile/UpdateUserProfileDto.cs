using System;
using System.ComponentModel.DataAnnotations;

namespace MediTrack.Application.Dtos.UserProfile
{
    public class UpdateUserProfileDto
    {
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [StringLength(10)] // Assuming Gender is stored as string like "Male", "Female", "Other"
        public string Gender { get; set; }

        [Url] // Validate if it's a valid URL format
        public string? AvatarUrl { get; set; }
    }
}
