using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PollApp.ViewModels
{
    public class PollViewModel
    {
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Title must be between 5 and 100 characters.")]
        public string Title { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "At least two options are required.")]
        [MinLength(2, ErrorMessage = "At least two options are required.")]
        public List<string> Options { get; set; } = new List<string>();
    }
}