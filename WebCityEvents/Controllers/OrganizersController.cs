using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebCityEvents.Data;
using WebCityEvents.Models;
using WebCityEvents.ViewModels;

namespace WebCityEvents.Controllers
{
    public class OrganizersController : Controller
    {
        private readonly EventContext _context;
        private const int PageSize = 20;

        public OrganizersController(EventContext context)
        {
            _context = context;
        }

        // GET: Organizers
        public async Task<IActionResult> Index(string searchName, int page = 1)
        {
            if (string.IsNullOrEmpty(searchName))
            {
                searchName = Request.Cookies["SearchName"] ?? "";
            }

            if (page <= 0)
            {
                page = int.TryParse(Request.Cookies["Page"], out int savedPage) ? savedPage : 1;
            }

            Response.Cookies.Append("SearchName", searchName, new CookieOptions { Expires = DateTimeOffset.UtcNow.AddDays(1) });
            Response.Cookies.Append("Page", page.ToString(), new CookieOptions { Expires = DateTimeOffset.UtcNow.AddDays(1) });

            var query = _context.Organizers.AsQueryable();
            if (!string.IsNullOrEmpty(searchName))
            {
                query = query.Where(o => o.FullName.Contains(searchName));
            }

            var totalOrganizers = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalOrganizers / (double)PageSize);

            var organizers = await query
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .Select(o => new OrganizerViewModel
                {
                    OrganizerID = o.OrganizerID,
                    FullName = o.FullName,
                    Post = o.Post
                })
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.SearchName = searchName;

            return View(organizers);
        }

        // GET: Organizers/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var organizer = await _context.Organizers.FindAsync(id);
            if (organizer == null)
            {
                return NotFound();
            }

            var organizerViewModel = new OrganizerViewModel
            {
                OrganizerID = organizer.OrganizerID,
                FullName = organizer.FullName,
                Post = organizer.Post
            };

            return View(organizerViewModel);
        }

        // GET: Organizers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Organizers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrganizerViewModel model)
        {
            if (ModelState.IsValid)
            {
                var organizer = new Organizer
                {
                    FullName = model.FullName,
                    Post = model.Post
                };
                _context.Organizers.Add(organizer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: Organizers/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var organizer = await _context.Organizers.FindAsync(id);
            if (organizer == null)
            {
                return NotFound();
            }

            var organizerViewModel = new OrganizerViewModel
            {
                OrganizerID = organizer.OrganizerID,
                FullName = organizer.FullName,
                Post = organizer.Post
            };

            return View(organizerViewModel);
        }

        // POST: Organizers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(OrganizerViewModel model)
        {
            if (ModelState.IsValid)
            {
                var organizer = await _context.Organizers.FindAsync(model.OrganizerID);
                if (organizer == null)
                {
                    return NotFound();
                }

                organizer.FullName = model.FullName;
                organizer.Post = model.Post;

                _context.Update(organizer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: Organizers/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var organizer = await _context.Organizers.FindAsync(id);
            if (organizer == null)
            {
                return NotFound();
            }

            var model = new OrganizerViewModel
            {
                OrganizerID = organizer.OrganizerID,
                FullName = organizer.FullName,
                Post = organizer.Post
            };

            return View(model);
        }

        // POST: Organizers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var organizer = await _context.Organizers.FindAsync(id);
            if (organizer != null)
            {
                _context.Organizers.Remove(organizer);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        public bool OrganizerExists(int id)
        {
            return _context.Organizers.Any(e => e.OrganizerID == id);
        }

        public IActionResult ClearFilters()
        {
            Response.Cookies.Delete("SearchName");
            Response.Cookies.Delete("Page");
            return RedirectToAction(nameof(Index));
        }
    }
}