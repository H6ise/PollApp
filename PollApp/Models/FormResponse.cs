using System;
using System.Collections.Generic;

namespace PollApp.Models
{
    public class FormResponse
    {
        public int Id { get; set; }
        public int PollId { get; set; }
        public Poll Poll { get; set; }
        public string? UserId { get; set; }
        public AppUser? User { get; set; }
        public string? ResponderEmail { get; set; }
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
        public ICollection<FormAnswer> Answers { get; set; } = new List<FormAnswer>();
    }
}

