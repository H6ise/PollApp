// VoteService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PollApp.Data;
using PollApp.Hubs;
using PollApp.Models;

namespace PollApp.Services
{
    public class VoteService : IVoteService
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<ResultsHub> _hubContext;

        public VoteService(AppDbContext context, IHubContext<ResultsHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task<bool> CanVoteAsync(string userId, int pollId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return false;
            }

            var poll = await _context.Polls.FirstOrDefaultAsync(p => p.Id == pollId);
            if (poll == null || !poll.IsActive || (poll.EndDate != null && poll.EndDate < DateTime.UtcNow))
            {
                return false;
            }

            var hasVoted = await _context.Votes
                .AnyAsync(v => v.UserId == userId && v.Option.PollId == pollId);

            return !hasVoted;
        }

        public async Task VoteAsync(string userId, int optionId)
        {
            var option = await _context.Options
                .Include(o => o.Poll)
                .FirstOrDefaultAsync(o => o.Id == optionId);

            if (option == null)
            {
                throw new ArgumentException("Invalid option ID.");
            }

            if (!await CanVoteAsync(userId, option.PollId))
            {
                throw new InvalidOperationException("User cannot vote in this poll.");
            }

            var vote = new Vote
            {
                UserId = userId,
                OptionId = optionId,
                VoteDate = DateTime.UtcNow
            };

            _context.Votes.Add(vote);
            await _context.SaveChangesAsync();

            // Get updated results
            var results = await GetResultsAsync(option.PollId);

            // Send to group
            await _hubContext.Clients.Group($"poll_{option.PollId}").SendAsync("UpdateResults", option.PollId, results.ToDictionary(r => r.Key.Text, r => r.Value));
        }

        private async Task<Dictionary<Option, int>> GetResultsAsync(int pollId)
        {
            var options = await _context.Options
                .Where(o => o.PollId == pollId)
                .Include(o => o.Votes)
                .ToListAsync();

            return options.ToDictionary(o => o, o => o.Votes?.Count ?? 0);
        }
    }
}