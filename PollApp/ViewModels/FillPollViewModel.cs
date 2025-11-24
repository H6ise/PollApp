using System.Collections.Generic;
using PollApp.Models;

namespace PollApp.ViewModels
{
    public class FillPollViewModel
    {
        public Poll Poll { get; set; }
        public List<Question> Questions { get; set; } = new();
        public bool CanEdit { get; set; }
        public bool ShowSuccess { get; set; }
        public bool ShowError { get; set; }
        public string? ErrorMessage { get; set; }
    }
}

