using System.Collections.Generic;
using System.Threading.Tasks;
using PollApp.Models;
using PollApp.ViewModels;

namespace PollApp.Services
{
    public interface IPollService
    {
        Task<Poll> CreatePollAsync(PollViewModel model);
        Task<List<Poll>> GetActivePollsAsync(int page, int pageSize);
        Task<Dictionary<Option, int>> GetResultsAsync(int pollId);
        Task<Poll> GetPollByIdAsync(int id);
    }
}