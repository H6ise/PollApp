using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PollApp.Models
{
    public class Option
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Option text is required.")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "Option text must be between 1 and 200 characters.")]
        public string Text { get; set; }
        public int PollId { get; set; }
        public virtual Poll Poll { get; set; }
        public virtual ICollection<Vote> Votes { get; set; } = new List<Vote>();
    }
}