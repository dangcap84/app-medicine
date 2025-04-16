using System;
using System.Threading.Tasks;
using MediTrack.Application.Dtos.UserProfile;
using MediTrack.Application.Interfaces;
using MediTrack.Domain.Entities;
using MediTrack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MediTrack.Infrastructure.Services
{
    public class UserProfileService : IUserProfileService
    {
        private readonly ApplicationDbContext _context;

        public UserProfileService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UserProfileDto?> GetUserProfileAsync(Guid userId)
        {
            var userProfile = await _context.UserProfiles
                .AsNoTracking() // Read-only operation
                .FirstOrDefaultAsync(up => up.UserId == userId);

            if (userProfile == null)
            {
                return null;
            }

            // Manual mapping (Consider using AutoMapper for larger projects)
            return new UserProfileDto
            {
                UserId = userProfile.UserId,
                FirstName = userProfile.FirstName,
                LastName = userProfile.LastName,
                DateOfBirth = userProfile.DateOfBirth,
                Gender = userProfile.Gender,
                AvatarUrl = userProfile.AvatarUrl
            };
        }

        public async Task<bool> UpdateUserProfileAsync(Guid userId, UpdateUserProfileDto userProfileDto)
        {
            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(up => up.UserId == userId);

            if (userProfile == null)
            {
                // Optionally, you could create a profile if it doesn't exist,
                // but current design assumes profile is created during registration or later.
                return false;
            }

            // Update properties
            userProfile.FirstName = userProfileDto.FirstName;
            userProfile.LastName = userProfileDto.LastName;
            userProfile.DateOfBirth = userProfileDto.DateOfBirth;
            userProfile.Gender = userProfileDto.Gender;
            userProfile.AvatarUrl = userProfileDto.AvatarUrl;
            userProfile.UpdatedAt = DateTime.UtcNow; // Update timestamp

            try
            {
                _context.UserProfiles.Update(userProfile);
                var result = await _context.SaveChangesAsync();
                return result > 0; // Return true if at least one record was affected
            }
            catch (DbUpdateConcurrencyException)
            {
                // Handle concurrency issues if necessary
                return false;
            }
            catch (DbUpdateException)
            {
                // Handle other potential database update errors
                return false;
            }
        }
    }
}
