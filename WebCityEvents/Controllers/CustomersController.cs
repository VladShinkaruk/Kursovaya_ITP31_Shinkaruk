using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebCityEvents.Data;
using WebCityEvents.Models;
using WebCityEvents.ViewModels;

namespace WebCityEvents.Controllers
{
    public class CustomersController : Controller
    {
        private readonly EventContext _context;
        private const int PageSize = 20;

        public CustomersController(EventContext context)
        {
            _context = context;
        }

        // GET: Customers
        public async Task<IActionResult> Index(string searchName, int page = 1)
        {
            searchName ??= HttpContext.Session.GetString("SearchName") ?? "";
            page = page <= 0 ? HttpContext.Session.GetInt32("Page") ?? 1 : page;

            HttpContext.Session.SetString("SearchName", searchName);
            HttpContext.Session.SetInt32("Page", page);

            var query = _context.Customers.AsQueryable();
            if (!string.IsNullOrEmpty(searchName))
            {
                query = query.Where(c => c.FullName.Contains(searchName));
            }

            var totalCustomers = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCustomers / (double)PageSize);

            var customers = await query
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .Select(c => new CustomerViewModel
                {
                    CustomerID = c.CustomerID,
                    FullName = c.FullName,
                    PassportData = c.PassportData
                })
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.SearchName = searchName;

            return View(customers);
        }

        // GET: Customers/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            var customerViewModel = new CustomerViewModel
            {
                CustomerID = customer.CustomerID,
                FullName = customer.FullName,
                PassportData = customer.PassportData
            };

            return View(customerViewModel);
        }

        // GET: Customers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Customers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CustomerViewModel model)
        {
            if (ModelState.IsValid)
            {
                var customer = new Customer
                {
                    FullName = model.FullName,
                    PassportData = model.PassportData
                };
                _context.Customers.Add(customer);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: Customers/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            var customerViewModel = new CustomerViewModel
            {
                CustomerID = customer.CustomerID,
                FullName = customer.FullName,
                PassportData = customer.PassportData
            };

            return View(customerViewModel);
        }

        // POST: Customers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CustomerViewModel customerViewModel)
        {
            if (ModelState.IsValid)
            {
                var customer = await _context.Customers.FindAsync(customerViewModel.CustomerID);
                if (customer == null)
                {
                    return NotFound();
                }

                customer.FullName = customerViewModel.FullName;
                customer.PassportData = customerViewModel.PassportData;

                _context.Update(customer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(customerViewModel);
        }

        // GET: Customers/Delete/5
        public IActionResult Delete(int id)
        {
            var customer = _context.Customers.Find(id);
            if (customer == null)
            {
                return NotFound();
            }

            var model = new CustomerViewModel
            {
                CustomerID = customer.CustomerID,
                FullName = customer.FullName,
                PassportData = customer.PassportData
            };
            return View(model);
        }

        // POST: Customers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var customer = _context.Customers.Find(id);
            if (customer != null)
            {
                _context.Customers.Remove(customer);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }

        public bool CustomerExists(int id)
        {
            return _context.Customers.Any(e => e.CustomerID == id);
        }

        public IActionResult ClearSession()
        {
            HttpContext.Session.Clear();
            return RedirectToAction(nameof(Index));
        }
    }
}