using System;
using System.ComponentModel.DataAnnotations;

namespace PollApp.Models
{
    public class Vote
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        public virtual AppUser User { get; set; }

        [Required]
        public int OptionId { get; set; }

        public virtual Option Option { get; set; }

        public DateTime VotedAt { get; set; } = DateTime.UtcNow;
    }
}