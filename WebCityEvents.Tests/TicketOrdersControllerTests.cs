using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace WebCityEvents.Tests
{
    public class TicketOrdersControllerTests
    {
        private readonly DbContextOptions<EventContext> _options;

        public TicketOrdersControllerTests()
        {
            _options = new DbContextOptionsBuilder<EventContext>()
                .UseInMemoryDatabase(databaseName: "TestTicketOrderDatabase")
                .Options;
        }

        private EventContext CreateContext()
        {
            return new EventContext(_options);
        }

        private void SeedDatabase(EventContext context)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            context.TicketOrders.AddRange(TestDataHelper.GetFakeTicketOrdersList());
            context.SaveChanges();
        }

        private TicketOrdersController CreateControllerWithSession(EventContext context)
        {
            var controller = new TicketOrdersController(context);
            var httpContext = new DefaultHttpContext
            {
                Session = new MockHttpSession()
            };
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            return controller;
        }

        [Fact]
        public async Task Index_ReturnsAllTicketOrders()
        {
            using var context = CreateContext();
            SeedDatabase(context);
            var controller = CreateControllerWithSession(context);

            var result = await controller.Index() as ViewResult;

            var model = Assert.IsAssignableFrom<List<TicketOrderViewModel>>(result.Model);
            Assert.Equal(2, model.Count);
        }

        [Fact]
        public async Task Index_FiltersTicketOrders()
        {
            using var context = CreateContext();
            SeedDatabase(context);
            var controller = CreateControllerWithSession(context);

            var result = await controller.Index(customerName: "John Doe") as ViewResult;

            var model = Assert.IsAssignableFrom<List<TicketOrderViewModel>>(result.Model);
            Assert.Single(model);
            Assert.Equal("John Doe", model.First().CustomerName);
        }

        [Fact]
        public async Task Details_ReturnsTicketOrderViewModel()
        {
            using var context = CreateContext();
            SeedDatabase(context);
            var controller = new TicketOrdersController(context);

            var result = await controller.Details(1) as ViewResult;

            Assert.NotNull(result);
            var model = Assert.IsType<TicketOrderViewModel>(result.Model);
            Assert.Equal(1, model.OrderID);
            Assert.Equal("Concert", model.EventName);
        }

        [Fact]
        public async Task Details_ReturnsNotFound()
        {
            using var context = CreateContext();
            SeedDatabase(context);
            var controller = new TicketOrdersController(context);

            var result = await controller.Details(999);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Create_ReturnsViewResult()
        {
            using var context = CreateContext();
            var controller = new TicketOrdersController(context);

            var result = controller.Create();

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Create_CreatesTicketOrder()
        {
            using var context = CreateContext();
            SeedDatabase(context);
            var controller = CreateControllerWithSession(context);

            var model = new TicketOrderViewModel
            {
                EventID = 1,
                CustomerID = 1,
                TicketCount = 2,
                OrderDate = DateTime.Now
            };

            var result = await controller.Create(model);

            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);

            var newOrder = context.TicketOrders.FirstOrDefault(o =>
                o.EventID == model.EventID &&
                o.CustomerID == model.CustomerID &&
                o.TicketCount == model.TicketCount);

            Assert.NotNull(newOrder);
            Assert.Equal(model.TicketCount, newOrder.TicketCount);
        }

        [Fact]
        public async Task Create_ReturnsViewWithModelError()
        {
            using var context = CreateContext();
            SeedDatabase(context);
            var controller = CreateControllerWithSession(context);

            var model = new TicketOrderViewModel
            {
                EventID = 999,
                CustomerID = 1,
                TicketCount = 2,
                OrderDate = DateTime.Now
            };

            var result = await controller.Create(model);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.True(controller.ModelState.ContainsKey(""));
            Assert.Equal("Мероприятие не найдено.", controller.ModelState[""].Errors.First().ErrorMessage);
        }

        [Fact]
        public async Task Edit_ReturnsTicketOrderForEdit()
        {
            using var context = CreateContext();
            SeedDatabase(context);
            var controller = CreateControllerWithSession(context);

            int existingOrderId = 1;
            var result = await controller.Edit(existingOrderId);
            var viewResult = Assert.IsType<ViewResult>(result);

            var model = Assert.IsType<TicketOrderViewModel>(viewResult.Model);
            Assert.Equal(existingOrderId, model.OrderID);
            Assert.Equal(context.TicketOrders.First(o => o.OrderID == existingOrderId).EventID, model.EventID);
            Assert.Equal(context.TicketOrders.First(o => o.OrderID == existingOrderId).CustomerID, model.CustomerID);
            Assert.Equal(context.TicketOrders.First(o => o.OrderID == existingOrderId).TicketCount, model.TicketCount);
            Assert.Equal(context.TicketOrders.First(o => o.OrderID == existingOrderId).Event.EventName, model.EventName);

            var customerSelectList = Assert.IsType<SelectList>(viewResult.ViewData["CustomerID"]);
            var eventSelectList = Assert.IsType<SelectList>(viewResult.ViewData["EventID"]);

            Assert.Equal(model.CustomerID, customerSelectList.SelectedValue);
            Assert.Equal(model.EventID, eventSelectList.SelectedValue);
        }

        [Fact]
        public async Task Edit_UpdatesTicketOrder()
        {
            using var context = CreateContext();
            SeedDatabase(context);
            var controller = CreateControllerWithSession(context);
            int existingOrderId = 1;

            var updatedModel = new TicketOrderViewModel
            {
                OrderID = existingOrderId,
                EventID = 1,
                CustomerID = 2,
                OrderDate = DateTime.Now,
                TicketCount = 3
            };

            var result = await controller.Edit(existingOrderId, updatedModel);
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);

            var updatedOrder = await context.TicketOrders.FindAsync(existingOrderId);
            Assert.NotNull(updatedOrder);
            Assert.Equal(updatedModel.EventID, updatedOrder.EventID);
            Assert.Equal(updatedModel.CustomerID, updatedOrder.CustomerID);
            Assert.Equal(updatedModel.TicketCount, updatedOrder.TicketCount);
            Assert.Equal(updatedModel.OrderDate, updatedOrder.OrderDate);
        }

        [Fact]
        public async Task Edit_ReturnsNotFound()
        {
            using var context = CreateContext();
            SeedDatabase(context);
            var controller = new TicketOrdersController(context);

            var updatedOrder = new TicketOrderViewModel
            {
                OrderID = 999,
                CustomerName = "Non-existent Customer"
            };

            var result = await controller.Edit(999, updatedOrder);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_ReturnsTicketOrderForDelete()
        {
            using var context = CreateContext();
            SeedDatabase(context);
            var controller = new TicketOrdersController(context);

            var result = await controller.Delete(1) as ViewResult;

            var model = Assert.IsType<TicketOrderViewModel>(result.Model);
            Assert.Equal("Concert", model.EventName);
        }

        [Fact]
        public async Task Delete_RemovesTicketOrder()
        {
            using var context = CreateContext();
            SeedDatabase(context);
            var controller = new TicketOrdersController(context);

            var result = await controller.DeleteConfirmed(1);

            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(controller.Index), redirectToActionResult.ActionName);

            var deletedOrder = context.TicketOrders.Find(1);
            Assert.Null(deletedOrder);
        }

        [Fact]
        public void TicketOrderExists_ReturnsTrue()
        {
            using var context = CreateContext();
            SeedDatabase(context);
            var controller = CreateControllerWithSession(context);

            int existingOrderId = 1;

            bool exists = controller.TicketOrderExists(existingOrderId);

            Assert.True(exists);
        }

        [Fact]
        public void ClearFilters_ClearsCookies()
        {
            using var context = CreateContext();
            var controller = new TicketOrdersController(context);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            controller.HttpContext.Response.Cookies.Append("CustomerName", "TestCustomer");

            var result = controller.ClearFilters();

            Assert.Null(controller.HttpContext.Request.Cookies["CustomerName"]);
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(controller.Index), redirectToActionResult.ActionName);
        }

        [Fact]
        public async Task GetAvailableTickets_ReturnsAvailableTickets()
        {
            using var context = CreateContext();
            SeedDatabase(context);
            var controller = CreateControllerWithSession(context);

            int eventId = 1;

            var result = await controller.GetAvailableTickets(eventId);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var availableTickets = Assert.IsType<int>(jsonResult.Value);
            Assert.True(availableTickets >= 0);
        }
    }
}