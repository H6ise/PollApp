using System.Collections.Generic;
using System.Threading.Tasks;
using PollApp.Models;
using PollApp.ViewModels;

namespace PollApp.Services
{
    public interface IPollService
    {
        Task<Poll> CreatePollAsync(PollViewModel model);
        Task<Poll> SavePollAsync(Poll poll, string userId);
        Task<List<Poll>> GetActivePollsAsync(int page, int pageSize);
        Task<Dictionary<Option, int>> GetResultsAsync(int pollId);
        Task RecordVoteAsync(int pollId, int optionId, string voterIdentifier);
        Task<Poll> GetPollByIdAsync(int id);

        // 🆕 ДОБАВЛЕНО: Получить опросы, созданные конкретным пользователем
        Task<List<Poll>> GetPollsByCreatorAsync(string creatorUserId, int page, int pageSize);

        // 🆕 Управление опросом: переименование и удаление
        Task RenamePollAsync(int id, string newTitle, string userId);
        Task DeletePollAsync(int id, string userId);
    }
}