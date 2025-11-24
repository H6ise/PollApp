using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PollApp.Data;
using PollApp.Models;
using PollApp.ViewModels;
using System.IO;

namespace PollApp.Services
{
    public class PollService : IPollService
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly string _imagesPath;

        public PollService(AppDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
            // Директория для хранения изображений (wwwroot/images)
            _imagesPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "polls");
            if (!Directory.Exists(_imagesPath)) Directory.CreateDirectory(_imagesPath);
        }

        // *** Метод GetPollByIdAsync ***
        public async Task<Poll> GetPollByIdAsync(int id)
        {
            // Включаем Options и Creator для полной информации
            return await _context.Polls
                .Include(p => p.Options)
                .Include(p => p.Creator)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        // *** Метод SavePollAsync (Создание/Редактирование) ***
        public async Task<Poll> SavePollAsync(Poll poll, string userId)
        {
            if (poll.Options == null || !poll.Options.Any() || poll.Options.All(o => string.IsNullOrWhiteSpace(o.Text)))
            {
                throw new ArgumentException("Опрос должен содержать как минимум один вариант ответа.");
            }

            if (poll.Id == 0)
            {
                // --- Создание нового опроса ---
                poll.CreatorUserId = userId;
                poll.StartDate = DateTime.UtcNow;
                poll.IsActive = true;
                _context.Polls.Add(poll);

                poll.Options = poll.Options.Where(o => !string.IsNullOrWhiteSpace(o.Text)).ToList();
            }
            else
            {
                // --- Редактирование существующего опроса ---
                var existingPoll = await _context.Polls
                    .Include(p => p.Options)
                    .FirstOrDefaultAsync(p => p.Id == poll.Id);

                if (existingPoll == null)
                {
                    throw new KeyNotFoundException($"Опрос с ID {poll.Id} не найден.");
                }

                // Обновление основных свойств
                existingPoll.Title = poll.Title;
                existingPoll.Description = poll.Description;
                existingPoll.EndDate = poll.EndDate;
                existingPoll.IsActive = poll.IsActive;

                // Управление вариантами ответов (Options)
                var newOptions = poll.Options.Where(o => o.Id == 0 && !string.IsNullOrWhiteSpace(o.Text)).ToList();
                var deletedOptions = existingPoll.Options.Where(eo => !poll.Options.Any(o => o.Id == eo.Id)).ToList();
                var updatedOptions = poll.Options.Where(o => o.Id != 0 && !string.IsNullOrWhiteSpace(o.Text)).ToList();

                // Удаление (с проверкой на наличие голосов)
                foreach (var optionToDelete in deletedOptions)
                {
                    var hasVotes = await _context.Votes.AnyAsync(v => v.OptionId == optionToDelete.Id);
                    if (!hasVotes)
                    {
                        _context.Options.Remove(optionToDelete);
                    }
                    else
                    {
                        // При необходимости, можно здесь заблокировать удаление и бросить исключение
                    }
                }

                // Добавление новых
                foreach (var newOption in newOptions)
                {
                    existingPoll.Options.Add(new Option { Text = newOption.Text, PollId = existingPoll.Id });
                }

                // Обновление существующих
                foreach (var updatedOption in updatedOptions)
                {
                    var existingOption = existingPoll.Options.FirstOrDefault(o => o.Id == updatedOption.Id);
                    if (existingOption != null)
                    {
                        existingOption.Text = updatedOption.Text;
                    }
                }
            }

            await _context.SaveChangesAsync();
            _cache.Remove("ActivePolls_*");

            return poll;
        }

        // *** Метод RecordVoteAsync (Голосование) ***
        public async Task RecordVoteAsync(int pollId, int optionId, string voterIdentifier)
        {
            var poll = await _context.Polls
                .Include(p => p.Options)
                .FirstOrDefaultAsync(p => p.Id == pollId);

            if (poll == null || !poll.IsActive || (poll.EndDate.HasValue && poll.EndDate.Value < DateTime.UtcNow))
            {
                throw new ArgumentException("Опрос не найден или уже завершен.");
            }

            var option = poll.Options.FirstOrDefault(o => o.Id == optionId);

            if (option == null)
            {
                throw new ArgumentException("Выбранный вариант не существует.");
            }

            // Определяем, является ли voterIdentifier ID пользователя (простая эвристика)
            // Применяется, если вы используете voterIdentifier как UserId для авторизованных
            bool isAuthorizedUserVote = !string.IsNullOrEmpty(voterIdentifier) && voterIdentifier.Length > 10; // Простая проверка

            // Логика предотвращения повторного голосования (проверяем по UserId)
            var hasVoted = await _context.Votes
                .Include(v => v.Option)
                .AnyAsync(v => v.Option.PollId == pollId && v.UserId == (isAuthorizedUserVote ? voterIdentifier : null));

            if (hasVoted)
            {
                throw new ArgumentException("Вы уже голосовали в этом опросе.");
            }

            // Создание и добавление голоса
            var vote = new Vote
            {
                OptionId = optionId,
                // Записываем ID пользователя в UserId
                UserId = isAuthorizedUserVote ? voterIdentifier : null,
                VoteDate = DateTime.UtcNow
            };

            _context.Votes.Add(vote);
            await _context.SaveChangesAsync();

            _cache.Remove($"PollResults_{pollId}");
        }

        // *** Метод GetActivePollsAsync ***
        public async Task<List<Poll>> GetActivePollsAsync(int page, int pageSize, string ownerFilter = null, string sort = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            IQueryable<Poll> query = _context.Polls.Include(p => p.Options);

            // фильтр по активности
            query = query.Where(p => p.IsActive && (p.EndDate == null || p.EndDate > DateTime.UtcNow));

            // фильтр по владельцу
            if (!string.IsNullOrEmpty(ownerFilter) && ownerFilter == "me")
            {
                // ownerFilter будет обработан на уровне контроллера где доступен userId
            }

            // Сортировка
            query = sort switch
            {
                "dateview" => query.OrderByDescending(p => p.LastViewed ?? p.StartDate),
                "mymod" => query.OrderByDescending(p => p.LastModified ?? p.StartDate),
                "datechanged" => query.OrderByDescending(p => p.LastModified ?? p.StartDate),
                "title" => query.OrderBy(p => p.Title),
                _ => query.OrderByDescending(p => p.StartDate)
            };

            var result = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return result;
        }

        // *** Метод GetResultsAsync ***
        public async Task<Dictionary<Option, int>> GetResultsAsync(int pollId)
        {
            var cacheKey = $"PollResults_{pollId}";
            if (!_cache.TryGetValue(cacheKey, out Dictionary<Option, int> results))
            {
                var options = await _context.Options
                    .Where(o => o.PollId == pollId)
                    .Include(o => o.Votes) // Включаем голоса для подсчета
                    .ToListAsync();

                results = options.ToDictionary(o => o, o => o.Votes?.Count ?? 0);

                var cacheOptions = new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(1) // Результаты обновляются чаще
                };
                _cache.Set(cacheKey, results, cacheOptions);
            }

            return results;
        }

        // Метод CreatePollAsync (оставлен для обратной совместимости, но SavePollAsync лучше)
        public async Task<Poll> CreatePollAsync(PollViewModel model)
        {
            // Логика осталась без изменений
            if (model.Options == null || model.Options.Count < 2 || model.Options.Any(string.IsNullOrWhiteSpace))
            {
                throw new ArgumentException("At least two non-empty options are required.");
            }

            var poll = new Poll
            {
                Title = model.Title,
                Description = model.Description,
                StartDate = DateTime.UtcNow,
                EndDate = model.EndDate,
                IsActive = true
            };

            _context.Polls.Add(poll);
            await _context.SaveChangesAsync();

            foreach (var optionText in model.Options.Where(o => !string.IsNullOrWhiteSpace(o)))
            {
                _context.Options.Add(new Option { Text = optionText, PollId = poll.Id });
            }

            await _context.SaveChangesAsync();
            return poll;
        }
        // *** НОВЫЙ МЕТОД: Получение опросов по создателю ***
        public async Task<List<Poll>> GetPollsByCreatorAsync(string creatorUserId, int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            // 1. Фильтруем по CreatorUserId
            // 2. Включаем Options
            // 3. Сортируем и применяем пагинацию
            return await _context.Polls
                .Where(p => p.CreatorUserId == creatorUserId)
                .Include(p => p.Options)
                .OrderByDescending(p => p.StartDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        // *** НОВЫЕ МЕТОДЫ: Rename и Delete ***
        public async Task RenamePollAsync(int id, string newTitle, string userId)
        {
            var poll = await _context.Polls.FirstOrDefaultAsync(p => p.Id == id);
            if (poll == null) throw new KeyNotFoundException("Poll not found");
            if (poll.CreatorUserId != userId) throw new UnauthorizedAccessException("Only owner can rename this poll");
            poll.Title = newTitle;
            await _context.SaveChangesAsync();
            _cache.Remove("ActivePolls_*");
        }

        public async Task DeletePollAsync(int id, string userId)
        {
            var poll = await _context.Polls.Include(p => p.Options).FirstOrDefaultAsync(p => p.Id == id);
            if (poll == null) throw new KeyNotFoundException("Poll not found");
            if (poll.CreatorUserId != userId) throw new UnauthorizedAccessException("Only owner can delete this poll");

            // Удалим опции и голоса (CASCADE не гарантируем)
            var optionIds = poll.Options.Select(o => o.Id).ToList();
            var votes = _context.Votes.Where(v => optionIds.Contains(v.OptionId));
            _context.Votes.RemoveRange(votes);
            _context.Options.RemoveRange(poll.Options);
            _context.Polls.Remove(poll);
            await _context.SaveChangesAsync();
            _cache.Remove("ActivePolls_*");
        }

        public async Task<string> SaveHeaderImageAsync(int pollId, byte[] data, string fileName, string userId)
        {
            var poll = await _context.Polls.FirstOrDefaultAsync(p => p.Id == pollId);
            if (poll == null) throw new KeyNotFoundException("Poll not found");
            if (poll.CreatorUserId != userId) throw new UnauthorizedAccessException("Only owner can change header image");

            var safeName = $"poll_{pollId}_{Guid.NewGuid().ToString("N")}{Path.GetExtension(fileName)}";
            var fullPath = Path.Combine(_imagesPath, safeName);
            await File.WriteAllBytesAsync(fullPath, data);

            var url = $"/images/polls/{safeName}";
            poll.HeaderImageUrl = url;
            poll.LastModified = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            _cache.Remove("ActivePolls_*");
            return url;
        }

        public async Task UpdateOrderAsync(Dictionary<int, int> orderById, string userId)
        {
            var ids = orderById.Keys.ToList();
            var polls = await _context.Polls.Where(p => ids.Contains(p.Id) && p.CreatorUserId == userId).ToListAsync();
            foreach (var p in polls)
            {
                if (orderById.TryGetValue(p.Id, out var ord)) p.Order = ord;
            }
            await _context.SaveChangesAsync();
        }

        public async Task MarkViewedAsync(int pollId, string userId)
        {
            var poll = await _context.Polls.FirstOrDefaultAsync(p => p.Id == pollId);
            if (poll == null) return;
            poll.LastViewed = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task MarkModifiedAsync(int pollId, string userId)
        {
            var poll = await _context.Polls.FirstOrDefaultAsync(p => p.Id == pollId);
            if (poll == null) return;
            poll.LastModified = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}