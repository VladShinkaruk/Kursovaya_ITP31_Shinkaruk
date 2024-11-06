using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebCityEvents.Data;
using WebCityEvents.Models;
using WebCityEvents.ViewModels;

namespace WebCityEvents.Controllers
{
    public class PlacesController : Controller
    {
        private readonly EventContext _context;
        private const int PageSize = 20;

        public PlacesController(EventContext context)
        {
            _context = context;
        }

        // GET: Places
        public async Task<IActionResult> Index(string searchPlace, int page = 1)
        {
            if (string.IsNullOrEmpty(searchPlace))
            {
                searchPlace = Request.Cookies["SearchPlace"] ?? "";
            }

            if (page <= 0)
            {
                page = int.TryParse(Request.Cookies["Page"], out int savedPage) ? savedPage : 1;
            }

            Response.Cookies.Append("SearchPlace", searchPlace, new CookieOptions { Expires = DateTimeOffset.UtcNow.AddDays(1) });
            Response.Cookies.Append("Page", page.ToString(), new CookieOptions { Expires = DateTimeOffset.UtcNow.AddDays(1) });

            var query = _context.Places.AsQueryable();
            if (!string.IsNullOrEmpty(searchPlace))
            {
                query = query.Where(p => p.PlaceName.Contains(searchPlace));
            }

            var totalPlaces = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalPlaces / (double)PageSize);

            var places = await query
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .Select(p => new PlaceViewModel
                {
                    PlaceID = p.PlaceID,
                    PlaceName = p.PlaceName,
                    Geolocation = p.Geolocation
                })
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.SearchPlace = searchPlace;

            return View(places);
        }

        // GET: Places/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var place = await _context.Places.FindAsync(id);
            if (place == null)
            {
                return NotFound();
            }

            var placeViewModel = new PlaceViewModel
            {
                PlaceID = place.PlaceID,
                PlaceName = place.PlaceName,
                Geolocation = place.Geolocation
            };

            return View(placeViewModel);
        }

        // GET: Places/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Places/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PlaceViewModel model)
        {
            if (ModelState.IsValid)
            {
                var place = new Place
                {
                    PlaceName = model.PlaceName,
                    Geolocation = model.Geolocation
                };
                _context.Places.Add(place);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: Places/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var place = await _context.Places.FindAsync(id);
            if (place == null)
            {
                return NotFound();
            }

            var placeViewModel = new PlaceViewModel
            {
                PlaceID = place.PlaceID,
                PlaceName = place.PlaceName,
                Geolocation = place.Geolocation
            };

            return View(placeViewModel);
        }

        // POST: Places/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PlaceViewModel placeViewModel)
        {
            if (ModelState.IsValid)
            {
                var place = await _context.Places.FindAsync(placeViewModel.PlaceID);
                if (place == null)
                {
                    return NotFound();
                }

                place.PlaceName = placeViewModel.PlaceName;
                place.Geolocation = placeViewModel.Geolocation;

                _context.Update(place);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(placeViewModel);
        }

        // GET: Places/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var place = await _context.Places.FindAsync(id);
            if (place == null)
            {
                return NotFound();
            }

            var placeViewModel = new PlaceViewModel
            {
                PlaceID = place.PlaceID,
                PlaceName = place.PlaceName,
                Geolocation = place.Geolocation
            };

            return View(placeViewModel);
        }

        // POST: Places/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var place = await _context.Places.FindAsync(id);
            if (place != null)
            {
                _context.Places.Remove(place);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        public bool PlaceExists(int id)
        {
            return _context.Places.Any(e => e.PlaceID == id);
        }

        public IActionResult ClearFilters()
        {
            Response.Cookies.Delete("SearchPlace");
            Response.Cookies.Delete("Page");
            return RedirectToAction(nameof(Index));
        }
    }
}