// PollController.cs
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

        public async Task<IActionResult> Index(int page = 1)
        {
            var polls = await _pollService.GetActivePollsAsync(page, 10);
            ViewBag.Page = page;
            ViewBag.HasNext = polls.Count == 10;
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
        public async Task<IActionResult> Vote(int optionId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                await _voteService.VoteAsync(userId, optionId);
                return Json(new { success = true, message = "Vote submitted successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> Results(int id)
        {
            var results = await _pollService.GetResultsAsync(id);
            var viewModel = results.ToDictionary(r => r.Key.Text, r => r.Value);
            ViewBag.PollId = id;
            return View(viewModel);
        }
    }
}