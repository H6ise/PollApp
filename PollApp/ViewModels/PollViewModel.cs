using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PollApp.ViewModels
{
    public class PollViewModel : Controller
    {
        // GET: PollViewModel
        public ActionResult Index()
        {
            return View();
        }

        // GET: PollViewModel/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: PollViewModel/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: PollViewModel/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: PollViewModel/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: PollViewModel/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: PollViewModel/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: PollViewModel/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
