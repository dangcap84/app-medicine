using System;
using System.ComponentModel.DataAnnotations;

namespace MediTrack.Application.Dtos.Notification
{
    public class UpdateNotificationDto
    {
        [Required]
        public bool IsRead { get; set; }
    }
}
