using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebCityEvents.Tests
{
    public class CustomersControllerTests
    {
        private readonly DbContextOptions<EventContext> _options;

        public CustomersControllerTests()
        {
            _options = new DbContextOptionsBuilder<EventContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
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

            context.Customers.AddRange(TestDataHelper.GetFakeCustomersList());
            context.SaveChanges();
        }

        private CustomersController CreateControllerWithSession(EventContext context)
        {
            var controller = new CustomersController(context);
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
        public async Task Index_ReturnsAllCustomers()
        {
            using var context = CreateContext();
            SeedDatabase(context);
            var controller = CreateControllerWithSession(context);

            var result = await controller.Index(null) as ViewResult;

            var model = Assert.IsAssignableFrom<List<CustomerViewModel>>(result.Model);
            Assert.Equal(3, model.Count);
        }

        [Fact]
        public async Task Index_FiltersCustomers()
        {
            using var context = CreateContext();
            SeedDatabase(context);
            var controller = CreateControllerWithSession(context);

            var result = await controller.Index("John") as ViewResult;

            var model = Assert.IsAssignableFrom<List<CustomerViewModel>>(result.Model);
            Assert.Single(model);
            Assert.Equal("John Doe", model.First().FullName);
        }

        [Fact]
        public async Task Details_ReturnsCustomerViewModel()
        {
            using var context = CreateContext();
            SeedDatabase(context);
            var controller = new CustomersController(context);

            var result = await controller.Details(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CustomerViewModel>(viewResult.Model);
            Assert.Equal(1, model.CustomerID);
            Assert.Equal("John Doe", model.FullName);
        }

        [Fact]
        public async Task Details_ReturnsNotFound()
        {
            using var context = CreateContext();
            SeedDatabase(context);
            var controller = new CustomersController(context);

            var result = await controller.Details(999);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Create_ReturnsViewResult()
        {
            using var context = CreateContext();
            var controller = new CustomersController(context);

            var result = controller.Create();

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Create_AddsNewCustomer()
        {
            using var context = CreateContext();
            SeedDatabase(context);
            var controller = new CustomersController(context);
            var newCustomer = new CustomerViewModel
            {
                FullName = "New Customer",
                PassportData = "NEW123456"
            };

            var result = controller.Create(newCustomer);

            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(controller.Index), redirectToActionResult.ActionName);

            var customer = context.Customers.FirstOrDefault(c => c.FullName == "New Customer");
            Assert.NotNull(customer);
        }

        [Fact]
        public void Create_ReturnsViewWithModelError()
        {
            using var context = CreateContext();
            var controller = new CustomersController(context);
            controller.ModelState.AddModelError("PassportData", "Required");

            var newCustomer = new CustomerViewModel
            {
                FullName = "Invalid Customer"
            };

            var result = controller.Create(newCustomer) as ViewResult;

            Assert.NotNull(result);
            Assert.IsType<CustomerViewModel>(result.Model);
            Assert.False(controller.ModelState.IsValid);
        }

        [Fact]
        public async Task Edit_ReturnsCustomerViewModel()
        {
            using var context = CreateContext();
            SeedDatabase(context);
            var controller = new CustomersController(context);

            var result = await controller.Edit(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CustomerViewModel>(viewResult.Model);
            Assert.Equal(1, model.CustomerID);
            Assert.Equal("John Doe", model.FullName);
        }

        [Fact]
        public async Task Edit_UpdatesCustomer()
        {
            using var context = CreateContext();
            SeedDatabase(context);
            var controller = new CustomersController(context);

            var updatedCustomer = new CustomerViewModel
            {
                CustomerID = 1,
                FullName = "Updated John Doe",
                PassportData = "AB123456"
            };

            var result = await controller.Edit(updatedCustomer);

            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(controller.Index), redirectToActionResult.ActionName);

            var customer = await context.Customers.FindAsync(1);
            Assert.Equal("Updated John Doe", customer.FullName);
        }

        [Fact]
        public async Task Edit_ReturnsNotFound()
        {
            using var context = CreateContext();
            SeedDatabase(context);
            var controller = new CustomersController(context);

            var nonExistentCustomer = new CustomerViewModel
            {
                CustomerID = 999,
                FullName = "Non-Existent Customer",
                PassportData = "XX000000"
            };

            var result = await controller.Edit(nonExistentCustomer);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Delete_ReturnsNotFound()
        {
            using var context = CreateContext();
            SeedDatabase(context);
            var controller = new CustomersController(context);

            var result = controller.Delete(999);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Delete_RemovesCustomer()
        {
            using var context = CreateContext();
            SeedDatabase(context);
            var controller = new CustomersController(context);

            var result = controller.DeleteConfirmed(1);

            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(controller.Index), redirectToActionResult.ActionName);

            var customer = context.Customers.Find(1);
            Assert.Null(customer);
        }

        [Fact]
        public void CustomerExists_ReturnsTrue()
        {
            using var context = CreateContext();
            SeedDatabase(context);
            var controller = new CustomersController(context);

            bool exists = controller.CustomerExists(1);

            Assert.True(exists);
        }

        [Fact]
        public void ClearSession_ClearsSession()
        {
            using var context = CreateContext();
            var controller = CreateControllerWithSession(context);

            controller.HttpContext.Session.SetString("TestKey", "TestValue");
            Assert.Equal("TestValue", controller.HttpContext.Session.GetString("TestKey"));

            var result = controller.ClearSession();

            Assert.Null(controller.HttpContext.Session.GetString("TestKey"));
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(controller.Index), redirectToActionResult.ActionName);
        }
    }
}