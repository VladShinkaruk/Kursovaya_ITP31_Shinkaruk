using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebCityEvents.Tests
{
    public class OrganizersControllerTests
    {
        private readonly DbContextOptions<EventContext> _options;

        public OrganizersControllerTests()
        {
            _options = new DbContextOptionsBuilder<EventContext>()
                .UseInMemoryDatabase(databaseName: "TestOrganizersDatabase")
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

            context.Organizers.AddRange(TestDataHelper.GetFakeOrganizersList());
            context.SaveChanges();
        }

        private OrganizersController CreateControllerWithSession(EventContext context)
        {
            var controller = new OrganizersController(context);
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
        public async Task Index_ReturnsAllOrganizers()
        {
            using var context = CreateContext();
            SeedDatabase(context);
            var controller = CreateControllerWithSession(context);

            var result = await controller.Index(null) as ViewResult;

            var model = Assert.IsAssignableFrom<List<OrganizerViewModel>>(result.Model);
            Assert.Equal(3, model.Count);
        }

        [Fact]
        public async Task Index_FiltersOrganizers()
        {
            using var context = CreateContext();
            SeedDatabase(context);
            var controller = CreateControllerWithSession(context);

            var result = await controller.Index("John Smith") as ViewResult;

            var model = Assert.IsAssignableFrom<List<OrganizerViewModel>>(result.Model);
            Assert.Single(model);
            Assert.Equal("John Smith", model.First().FullName);
        }

        [Fact]
        public async Task Details_ReturnsOrganizerViewModel()
        {
            using var context = CreateContext();
            SeedDatabase(context);
            var controller = new OrganizersController(context);

            var result = await controller.Details(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<OrganizerViewModel>(viewResult.Model);
            Assert.Equal(1, model.OrganizerID);
            Assert.Equal("John Smith", model.FullName);
        }

        [Fact]
        public async Task Details_ReturnsNotFound()
        {
            using var context = CreateContext();
            SeedDatabase(context);
            var controller = new OrganizersController(context);

            var result = await controller.Details(999);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Create_ReturnsViewResult()
        {
            using var context = CreateContext();
            var controller = new OrganizersController(context);

            var result = controller.Create();

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Create_AddsNewOrganizer()
        {
            using var context = CreateContext();
            SeedDatabase(context);
            var controller = new OrganizersController(context);
            var newOrganizer = new OrganizerViewModel
            {
                FullName = "New Organizer",
                Post = "Manager"
            };

            var result = await controller.Create(newOrganizer);

            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(controller.Index), redirectToActionResult.ActionName);

            var organizer = context.Organizers.FirstOrDefault(o => o.FullName == "New Organizer");
            Assert.NotNull(organizer);
        }

        [Fact]
        public async Task Create_ReturnsViewWithModelError()
        {
            using var context = CreateContext();
            var controller = new OrganizersController(context);

            controller.ModelState.AddModelError("Position", "Required");

            var model = new OrganizerViewModel
            {
                FullName = "Incomplete Organizer"
            };

            var result = await controller.Create(model) as ViewResult;

            Assert.NotNull(result);
            Assert.IsType<OrganizerViewModel>(result.Model);
            Assert.False(controller.ModelState.IsValid);
        }

        [Fact]
        public async Task Edit_ReturnsOrganizerViewModel()
        {
            using var context = CreateContext();
            SeedDatabase(context);
            var controller = new OrganizersController(context);

            var result = await controller.Edit(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<OrganizerViewModel>(viewResult.Model);
            Assert.Equal(1, model.OrganizerID);
            Assert.Equal("John Smith", model.FullName);
        }

        [Fact]
        public async Task Edit_UpdatesOrganizer()
        {
            using var context = CreateContext();
            SeedDatabase(context);
            var controller = new OrganizersController(context);

            var updatedOrganizer = new OrganizerViewModel
            {
                OrganizerID = 1,
                FullName = "Updated John Doe",
                Post = "Senior Manager"
            };

            var result = await controller.Edit(updatedOrganizer);

            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(controller.Index), redirectToActionResult.ActionName);

            var organizer = await context.Organizers.FindAsync(1);
            Assert.Equal("Updated John Doe", organizer.FullName);
        }

        [Fact]
        public async Task Edit_ReturnsNotFound()
        {
            using var context = CreateContext();
            SeedDatabase(context);
            var controller = new OrganizersController(context);

            var nonExistentOrganizer = new OrganizerViewModel
            {
                OrganizerID = 999,
                FullName = "Non-Existent Organizer",
                Post = "N/A"
            };

            var result = await controller.Edit(nonExistentOrganizer);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_ReturnsNotFound()
        {
            using var context = CreateContext();
            SeedDatabase(context);
            var controller = new OrganizersController(context);

            var result = await controller.Delete(999);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_RemovesOrganizer()
        {
            using var context = CreateContext();
            SeedDatabase(context);
            var controller = new OrganizersController(context);

            var result = await controller.DeleteConfirmed(1);

            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(controller.Index), redirectToActionResult.ActionName);

            var organizer = context.Organizers.Find(1);
            Assert.Null(organizer);
        }

        [Fact]
        public void OrganizerExists_ReturnsTrue()
        {
            using var context = CreateContext();
            SeedDatabase(context);
            var controller = new OrganizersController(context);

            bool exists = controller.OrganizerExists(1);

            Assert.True(exists);
        }

        [Fact]
        public void ClearFilters_ClearsCookies()
        {
            using var context = CreateContext();
            var cookiesMock = new Mock<IResponseCookies>();
            cookiesMock.Setup(c => c.Delete(It.IsAny<string>()));

            var responseMock = new Mock<HttpResponse>();
            responseMock.SetupGet(r => r.Cookies).Returns(cookiesMock.Object);

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.SetupGet(h => h.Response).Returns(responseMock.Object);

            var controller = new OrganizersController(context)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = httpContextMock.Object
                }
            };

            var result = controller.ClearFilters() as RedirectToActionResult;

            cookiesMock.Verify(c => c.Delete("SearchName"), Times.Once);
            cookiesMock.Verify(c => c.Delete("Page"), Times.Once);

            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
        }
    }
}