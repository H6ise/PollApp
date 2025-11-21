using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PollApp.Models
{
    public class Poll
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Title must be between 5 and 100 characters.")]
        public string Title { get; set; }
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
        public string HeaderImageUrl { get; set; } = "/images/default-header.png";  /* Шапка картинка */
        public virtual ICollection<Option> Options { get; set; } = new List<Option>();
    }
}