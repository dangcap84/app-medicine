using System;
using System.Security.Claims;
using System.Threading.Tasks;
using MediTrack.Application.Dtos.UserProfile;
using MediTrack.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediTrack.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Require authentication for all actions in this controller
    public class UserProfilesController : ControllerBase
    {
        private readonly IUserProfileService _userProfileService;

        public UserProfilesController(IUserProfileService userProfileService)
        {
            _userProfileService = userProfileService;
        }

        /// <summary>
        /// Gets the profile of the currently authenticated user.
        /// </summary>
        /// <returns>The user's profile.</returns>
        /// <response code="200">Returns the user's profile.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="404">If the user's profile is not found.</response>
        [HttpGet("me")] // Route to get the current user's profile
        [ProducesResponseType(typeof(UserProfileDto), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetMyProfile()
        {
            // Get the user ID from the claims principal
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out var userId))
            {
                return Unauthorized("Invalid user identifier."); // Should not happen if token is valid
            }

            var userProfile = await _userProfileService.GetUserProfileAsync(userId);

            if (userProfile == null)
            {
                return NotFound($"Profile for user ID {userId} not found.");
            }

            return Ok(userProfile);
        }

        /// <summary>
        /// Updates the profile of the currently authenticated user.
        /// </summary>
        /// <param name="updateUserProfileDto">The updated profile data.</param>
        /// <returns>No content if successful.</returns>
        /// <response code="204">If the profile was updated successfully.</response>
        /// <response code="400">If the provided data is invalid.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="404">If the user's profile is not found.</response>
        [HttpPut("me")] // Route to update the current user's profile
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateUserProfileDto updateUserProfileDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out var userId))
            {
                return Unauthorized("Invalid user identifier.");
            }

            var success = await _userProfileService.UpdateUserProfileAsync(userId, updateUserProfileDto);

            if (!success)
            {
                // Could be NotFound or another update failure
                // Check if profile exists first for a more specific error, but for simplicity:
                return NotFound($"Could not update profile for user ID {userId}. Profile might not exist or update failed.");
            }

            return NoContent(); // Standard response for successful PUT
        }
    }
}
