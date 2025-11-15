using System.Collections.Generic;

namespace PollApp.ViewModels
{
    public class ResultViewModel
    {
        public int PollId { get; set; }

        public string PollTitle { get; set; }

        public Dictionary<string, int> Results { get; set; } = new Dictionary<string, int>();

        public int TotalVotes => Results.Values.Sum();
    }
}