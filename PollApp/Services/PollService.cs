// PollService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PollApp.Data;
using PollApp.Models;
using PollApp.ViewModels;

namespace PollApp.Services
{
    public class PollService : IPollService
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;

        public PollService(AppDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<Poll> CreatePollAsync(PollViewModel model)
        {
            if (model.Options == null || model.Options.Count < 2 || model.Options.Any(string.IsNullOrWhiteSpace))
            {
                throw new ArgumentException("At least two non-empty options are required.");
            }

            var poll = new Poll
            {
                Title = model.Title,
                Description = model.Description,
                StartDate = DateTime.UtcNow,
                IsActive = true
            };

            _context.Polls.Add(poll);
            await _context.SaveChangesAsync();

            foreach (var optionText in model.Options)
            {
                if (!string.IsNullOrWhiteSpace(optionText))
                {
                    var option = new Option { Text = optionText, PollId = poll.Id };
                    _context.Options.Add(option);
                }
            }

            await _context.SaveChangesAsync();
            return poll;
        }

        public async Task<List<Poll>> GetActivePollsAsync(int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var cacheKey = $"ActivePolls_Page{page}_Size{pageSize}";
            if (!_cache.TryGetValue(cacheKey, out List<Poll> polls))
            {
                polls = await _context.Polls
                    .Where(p => p.IsActive && (p.EndDate == null || p.EndDate > DateTime.UtcNow))
                    .Include(p => p.Options)
                    .OrderByDescending(p => p.StartDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var cacheOptions = new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(5)
                };
                _cache.Set(cacheKey, polls, cacheOptions);
            }

            return polls;
        }

        public async Task<Dictionary<Option, int>> GetResultsAsync(int pollId)
        {
            var options = await _context.Options
                .Where(o => o.PollId == pollId)
                .Include(o => o.Votes)
                .ToListAsync();

            return options.ToDictionary(o => o, o => o.Votes?.Count ?? 0);
        }

        public async Task<Poll> GetPollByIdAsync(int id)
        {
            return await _context.Polls
                .Include(p => p.Options)
                .FirstOrDefaultAsync(p => p.Id == id);
        }
    }
}