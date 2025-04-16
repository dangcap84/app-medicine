using System;
using System.Collections.Generic;

namespace MediTrack.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public UserProfile? UserProfile { get; set; }
    public ICollection<Medicine> Medicines { get; set; } = new List<Medicine>();
    public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
