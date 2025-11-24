using System.Collections.Generic;

namespace PollApp.ViewModels
{
    public class QuestionAnswerInput
    {
        public int QuestionId { get; set; }
        public List<int> OptionIds { get; set; } = new();
        public string? TextAnswer { get; set; }
    }
}

