using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PollApp.Models;
using PollApp.Services;

namespace PollApp.Controllers
{
    [AllowAnonymous]
    public class PollController : Controller
    {
        private readonly IPollService _pollService;
        private readonly IVoteService _voteService;

        public PollController(IPollService pollService, IVoteService voteService)
        {
            _pollService = pollService;
            _voteService = voteService;
        }

        public async Task<IActionResult> Index(int page = 1, string sort = "dateview", string owner = "me")
        {
            string currentUserId = null;
            if (User.Identity.IsAuthenticated)
                currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var polls = await _pollService.GetActivePollsAsync(page, 50, owner, sort);

            // Если указан owner=me, фильтруем на сервере
            if (owner == "me" && currentUserId != null)
            {
                polls = polls.Where(p => p.CreatorUserId == currentUserId).ToList();
            }

            ViewBag.Page = page;
            ViewBag.Sort = sort;
            ViewBag.Owner = owner;
            ViewBag.HasNext = polls.Count == 50;
            return View(polls);
        }

        public async Task<IActionResult> Details(int id)
        {
            var poll = await _pollService.GetPollByIdAsync(id);
            if (poll == null || !poll.IsActive || (poll.EndDate != null && poll.EndDate < DateTime.UtcNow))
            {
                return NotFound();
            }
            return View(poll);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Vote(int optionId, int pollId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                await _voteService.VoteAsync(userId, optionId);
                // После голосования перенаправляем на результаты опроса
                return RedirectToAction("Results", "Poll", new { id = pollId });
            }
            catch (Exception ex)
            {
                // Можно добавить сообщение об ошибке во ViewBag или ModelState
                ModelState.AddModelError(string.Empty, ex.Message);
                // Получаем опрос для повторного отображения страницы Details
                var poll = await _pollService.GetPollByIdAsync(pollId);
                return View("Details", poll);
            }
        }

        public async Task<IActionResult> Results(int id)
        {
            var results = await _pollService.GetResultsAsync(id);
            var viewModel = results.ToDictionary(r => r.Key.Text, r => r.Value);
            ViewBag.PollId = id;
            return View(viewModel);
        }
        [Authorize]
        public async Task<IActionResult> CreateEdit(int? id)
        {
            Poll poll;
            if (id.HasValue)
            {
                // Редактирование существующего
                poll = await _pollService.GetPollByIdAsync(id.Value);
                // Проверяем, что текущий пользователь является создателем опроса
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (poll == null || poll.CreatorUserId != userId)
                {
                    return Forbid(); // Запрещаем редактирование чужих опросов
                }
            }
            else
            {
                // Создание нового
                poll = new Poll { Title = "Новая форма", Description = "Описание", Options = new List<Option>() };
                // Добавляем минимальный вариант ответа
                poll.Options.Add(new Option { Text = "Вариант 1" });
            }
            // Используем простую ViewModel, если она нужна, или саму модель
            return View(poll);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(Poll model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    await _pollService.SavePollAsync(model, userId);
                    return RedirectToAction("Details", new { id = model.Id });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "Ошибка сохранения: " + ex.Message);
                }
            }
            // Если ошибка, возвращаем пользователя обратно в конструктор
            return View("CreateEdit", model);
        }

        // Новые эндпоинты: Rename / Delete
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Rename(int id, [FromForm] string title)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                await _pollService.RenamePollAsync(id, title, userId);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                await _pollService.DeletePollAsync(id, userId);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UploadHeaderImage(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var file = Request.Form.Files.FirstOrDefault();
            if (file == null) return BadRequest("No file");
            using var ms = new System.IO.MemoryStream();
            await file.CopyToAsync(ms);
            var data = ms.ToArray();
            try
            {
                var url = await _pollService.SaveHeaderImageAsync(id, data, file.FileName, userId);
                return Ok(new { url });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UpdateOrder([FromBody] Dictionary<int,int> orderById)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                await _pollService.UpdateOrderAsync(orderById, userId);
                return Ok();
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }
    }
}