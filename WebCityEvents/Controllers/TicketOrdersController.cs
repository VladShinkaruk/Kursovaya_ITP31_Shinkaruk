using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebCityEvents.Data;
using WebCityEvents.Models;
using WebCityEvents.ViewModels;

namespace WebCityEvents.Controllers
{
    public class TicketOrdersController : Controller
    {
        private readonly EventContext _context;

        public TicketOrdersController(EventContext context)
        {
            _context = context;
        }

        // GET: TicketOrders
        public async Task<IActionResult> Index(string customerName = "", string eventName = "", int page = 1, string sortOrder = "OrderDate", string sortDirection = "asc")
        {
            const int pageSize = 20;

            if (string.IsNullOrEmpty(customerName))
            {
                customerName = Request.Cookies["CustomerName"] ?? string.Empty;
            }
            if (string.IsNullOrEmpty(eventName))
            {
                eventName = Request.Cookies["EventName"] ?? string.Empty;
            }

            Response.Cookies.Append("CustomerName", customerName);
            Response.Cookies.Append("EventName", eventName);

            var query = _context.TicketOrders
                .Include(t => t.Customer)
                .Include(t => t.Event)
                .AsQueryable();

            if (!string.IsNullOrEmpty(customerName))
            {
                query = query.Where(t => t.Customer.FullName.Contains(customerName));
            }
            if (!string.IsNullOrEmpty(eventName))
            {
                query = query.Where(t => t.Event.EventName.Contains(eventName));
            }

            query = sortOrder switch
            {
                "TicketCount" => sortDirection == "asc" ? query.OrderBy(t => t.TicketCount) : query.OrderByDescending(t => t.TicketCount),
                _ => sortDirection == "asc" ? query.OrderBy(t => t.OrderDate) : query.OrderByDescending(t => t.OrderDate),
            };

            var totalCount = await query.CountAsync();
            var orders = await query.Skip((page - 1) * pageSize)
                                     .Take(pageSize)
                                     .Select(t => new TicketOrderViewModel
                                     {
                                         OrderID = t.OrderID,
                                         CustomerName = t.Customer.FullName,
                                         CustomerPassportData = t.Customer.PassportData,
                                         EventName = t.Event.EventName,
                                         OrderDate = t.OrderDate.Date,
                                         TicketCount = t.TicketCount
                                     })
                                     .ToListAsync();

            if (!orders.Any())
            {
                ViewBag.Message = "По заданным фильтрам ничего не найдено";
            }

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            ViewBag.CustomerName = customerName;
            ViewBag.EventName = eventName;
            ViewBag.SortOrder = sortOrder;
            ViewBag.SortDirection = sortDirection;

            return View(orders);
        }

        // GET: TicketOrders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticketOrder = await _context.TicketOrders
                .Include(t => t.Customer)
                .Include(t => t.Event)
                .FirstOrDefaultAsync(m => m.OrderID == id);

            if (ticketOrder == null)
            {
                return NotFound();
            }

            var model = new TicketOrderViewModel
            {
                OrderID = ticketOrder.OrderID,
                CustomerName = ticketOrder.Customer.FullName,
                EventName = ticketOrder.Event.EventName,
                OrderDate = ticketOrder.OrderDate.Date,
                TicketCount = ticketOrder.TicketCount
            };

            return View(model);
        }

        // GET: TicketOrders/Create
        public IActionResult Create()
        {
            var model = new TicketOrderViewModel
            {
                OrderDate = DateTime.Now
            };
            ViewBag.CustomerID = new SelectList(_context.Customers, "CustomerID", "FullName");
            return View(model);
        }

        // POST: TicketOrders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TicketOrderViewModel model)
        {
            var eventItem = await _context.Events.FindAsync(model.EventID);
            if (eventItem == null)
            {
                ModelState.AddModelError("", "Мероприятие не найдено");
                return View(model);
            }

            if (model.OrderDate > eventItem.EventDate)
            {
                ModelState.AddModelError("", "Дата заказа не может быть позже даты проведения мероприятия");
                ViewBag.CustomerID = new SelectList(_context.Customers, "CustomerID", "FullName");
                return View(model);
            }

            var soldTickets = await _context.TicketOrders
                .Where(o => o.EventID == model.EventID)
                .SumAsync(o => o.TicketCount);

            var availableTickets = eventItem.TicketAmount - soldTickets;

            if (model.TicketCount > availableTickets)
            {
                ModelState.AddModelError("", "Недостаточно доступных билетов на данное мероприятие");
                ViewBag.CustomerID = new SelectList(_context.Customers, "CustomerID", "FullName");
                return View(model);
            }

            var newOrder = new TicketOrder
            {
                EventID = model.EventID,
                CustomerID = model.CustomerID,
                TicketCount = model.TicketCount,
                OrderDate = DateTime.Now
            };

            _context.TicketOrders.Add(newOrder);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: TicketOrders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticketOrder = await _context.TicketOrders
                .Include(t => t.Event)
                .Include(t => t.Customer)
                .FirstOrDefaultAsync(m => m.OrderID == id);

            if (ticketOrder == null)
            {
                return NotFound();
            }

            var model = new TicketOrderViewModel
            {
                OrderID = ticketOrder.OrderID,
                EventID = ticketOrder.EventID,
                CustomerID = ticketOrder.CustomerID,
                OrderDate = ticketOrder.OrderDate,
                TicketCount = ticketOrder.TicketCount,
                EventName = ticketOrder.Event.EventName

            };

            ViewData["CustomerID"] = new SelectList(_context.Customers, "CustomerID", "FullName", ticketOrder.CustomerID);
            ViewData["EventID"] = new SelectList(_context.Events, "EventID", "EventName", ticketOrder.EventID);

            return View(model);
        }

        // POST: TicketOrders/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrderID,EventID,CustomerID,OrderDate,TicketCount")] TicketOrderViewModel ticketOrderViewModel)
        {
            if (id != ticketOrderViewModel.OrderID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var eventItem = await _context.Events.FindAsync(ticketOrderViewModel.EventID);
                    if (eventItem == null)
                    {
                        return NotFound();
                    }

                    if (ticketOrderViewModel.OrderDate > eventItem.EventDate)
                    {
                        ModelState.AddModelError("", "Дата заказа не может быть позже даты проведения мероприятия");
                        ViewData["CustomerID"] = new SelectList(_context.Customers, "CustomerID", "FullName", ticketOrderViewModel.CustomerID);
                        ViewData["EventID"] = new SelectList(_context.Events, "EventID", "EventName", ticketOrderViewModel.EventID);
                        return View(ticketOrderViewModel);
                    }

                    var totalSoldTickets = await _context.TicketOrders
                        .Where(o => o.EventID == ticketOrderViewModel.EventID && o.OrderID != id)
                        .SumAsync(o => o.TicketCount);

                    var availableTickets = eventItem.TicketAmount - totalSoldTickets;

                    if (ticketOrderViewModel.TicketCount > availableTickets)
                    {
                        ModelState.AddModelError("", "Недостаточно доступных билетов на данное мероприятие");
                        ViewData["CustomerID"] = new SelectList(_context.Customers, "CustomerID", "FullName", ticketOrderViewModel.CustomerID);
                        ViewData["EventID"] = new SelectList(_context.Events, "EventID", "EventName", ticketOrderViewModel.EventID);
                        return View(ticketOrderViewModel);
                    }

                    var ticketOrder = await _context.TicketOrders.FindAsync(id);
                    if (ticketOrder == null)
                    {
                        return NotFound();
                    }

                    ticketOrder.EventID = ticketOrderViewModel.EventID;
                    ticketOrder.CustomerID = ticketOrderViewModel.CustomerID;
                    ticketOrder.OrderDate = ticketOrderViewModel.OrderDate;
                    ticketOrder.TicketCount = ticketOrderViewModel.TicketCount;

                    _context.Update(ticketOrder);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketOrderExists(ticketOrderViewModel.OrderID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["CustomerID"] = new SelectList(_context.Customers, "CustomerID", "FullName", ticketOrderViewModel.CustomerID);
            ViewData["EventID"] = new SelectList(_context.Events, "EventID", "EventName", ticketOrderViewModel.EventID);
            return View(ticketOrderViewModel);
        }

        // GET: TicketOrders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticketOrder = await _context.TicketOrders
                .Include(t => t.Customer)
                .Include(t => t.Event)
                .FirstOrDefaultAsync(m => m.OrderID == id);
            if (ticketOrder == null)
            {
                return NotFound();
            }

            var model = new TicketOrderViewModel
            {
                OrderID = ticketOrder.OrderID,
                CustomerName = ticketOrder.Customer.FullName,
                EventName = ticketOrder.Event.EventName,
                OrderDate = ticketOrder.OrderDate.Date,
                TicketCount = ticketOrder.TicketCount,
            };

            return View(model);
        }

        // POST: TicketOrders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ticketOrder = await _context.TicketOrders.FindAsync(id);
            if (ticketOrder != null)
            {
                _context.TicketOrders.Remove(ticketOrder);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public bool TicketOrderExists(int id)
        {
            return _context.TicketOrders.Any(e => e.OrderID == id);
        }

        public IActionResult ClearFilters()
        {
            Response.Cookies.Delete("CustomerName");
            Response.Cookies.Delete("EventName");

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<JsonResult> GetAvailableTickets(int eventId)
        {
            var eventItem = await _context.Events.FindAsync(eventId);
            var soldTickets = await _context.TicketOrders
                .Where(o => o.EventID == eventId)
                .SumAsync(o => o.TicketCount);

            var availableTickets = eventItem.TicketAmount - soldTickets;
            return Json(availableTickets);
        }

        [HttpGet]
        public async Task<IActionResult> SearchEvents(string term)
        {
            if (string.IsNullOrEmpty(term))
            {
                return Json(new List<string>());
            }

            var events = await _context.Events
                .Where(e => e.EventName.StartsWith(term))
                .Select(e => new { e.EventID, e.EventName })
                .ToListAsync();

            return Json(events);
        }

        [HttpGet]
        public async Task<JsonResult> GetEventDetails(int eventId)
        {
            var eventItem = await _context.Events
                .Where(e => e.EventID == eventId)
                .Select(e => new { e.EventDate, e.TicketAmount })
                .FirstOrDefaultAsync();

            if (eventItem == null)
            {
                return Json(new { availableTickets = 0, eventDate = (DateTime?)null });
            }

            var soldTickets = await _context.TicketOrders
                .Where(o => o.EventID == eventId)
                .SumAsync(o => o.TicketCount);

            var availableTickets = eventItem.TicketAmount - soldTickets;

            return Json(new { availableTickets, eventDate = eventItem.EventDate });
        }
    }
}