// IVoteService.cs
using System.Threading.Tasks;

namespace PollApp.Services
{
    public interface IVoteService
    {
        Task<bool> CanVoteAsync(string userId, int pollId);
        Task VoteAsync(string userId, int optionId);
    }
}