using System;
using System.Threading.Tasks;
using MediTrack.Application.Dtos.UserProfile;

namespace MediTrack.Application.Interfaces
{
    public interface IUserProfileService
    {
        Task<UserProfileDto?> GetUserProfileAsync(Guid userId);
        Task<bool> UpdateUserProfileAsync(Guid userId, UpdateUserProfileDto userProfileDto);
    }
}
