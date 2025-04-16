using System;

namespace MediTrack.Application.Dtos.UserProfile
{
    public class UserProfileDto
    {
        public Guid UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string? AvatarUrl { get; set; }
    }
}
