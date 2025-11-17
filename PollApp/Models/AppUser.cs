// AppUser.cs
using System;
using Microsoft.AspNetCore.Identity;

namespace PollApp.Models
{
    /// <summary>
    /// Custom user class extending IdentityUser for additional properties.
    /// </summary>
    public class AppUser : IdentityUser
    {
        // Additional properties for user profile
        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public string? ProfilePictureUrl { get; set; }

        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        // Navigation properties if needed (e.g., for votes)
        public virtual ICollection<Vote> Votes { get; set; } = new List<Vote>();
    }
}