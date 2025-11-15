using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PollApp.Data;
using PollApp.Models;
using PollApp.Services;
using PollApp.ViewModels;

namespace PollApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IPollService _pollService;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _context;

        public AdminController(IPollService pollService, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, AppDbContext context)
        {
            _pollService = pollService;
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            var polls = await _context.Polls
                .OrderByDescending(p => p.StartDate)
                .Skip((page - 1) * 10)
                .Take(10)
                .ToListAsync();
            ViewBag.Page = page;
            ViewBag.HasNext = polls.Count == 10;
            return View(polls);
        }

        public IActionResult Create()
        {
            return View(new PollViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PollViewModel model)
        {
            if (ModelState.IsValid)
            {
                await _pollService.CreatePollAsync(model);
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var poll = await _context.Polls
                .Include(p => p.Options)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (poll == null) return NotFound();

            var model = new PollViewModel
            {
                Title = poll.Title,
                Description = poll.Description,
                Options = poll.Options.Select(o => o.Text).ToList()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PollViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var poll = await _context.Polls
                .Include(p => p.Options)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (poll == null) return NotFound();

            poll.Title = model.Title;
            poll.Description = model.Description;

            _context.Options.RemoveRange(poll.Options);
            await _context.SaveChangesAsync();

            foreach (var opt in model.Options.Where(o => !string.IsNullOrWhiteSpace(o)))
            {
                _context.Options.Add(new Option { Text = opt, PollId = poll.Id });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var poll = await _context.Polls
                .Include(p => p.Options)
                .ThenInclude(o => o.Votes)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (poll != null)
            {
                foreach (var option in poll.Options)
                {
                    _context.Votes.RemoveRange(option.Votes);
                }
                _context.Options.RemoveRange(poll.Options);
                _context.Polls.Remove(poll);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();
            ViewData["Roles"] = await _roleManager.Roles.ToListAsync();
            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Block(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                await _userManager.SetLockoutEnabledAsync(user, true);
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
            }
            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignRole(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null && await _roleManager.RoleExistsAsync(role))
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRoleAsync(user, role);
            }
            return RedirectToAction(nameof(Users));
        }
    }
}