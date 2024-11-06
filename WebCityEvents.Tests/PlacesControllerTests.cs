using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebCityEvents.Tests
{
    public class PlacesControllerTests
    {
        private readonly DbContextOptions<EventContext> _options;

        public PlacesControllerTests()
        {
            _options = new DbContextOptionsBuilder<EventContext>()
                .UseInMemoryDatabase(databaseName: "TestPlacesDatabase")
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

            context.Places.AddRange(TestDataHelper.GetFakePlacesList());
            context.SaveChanges();
        }

        private PlacesController CreateControllerWithSession(EventContext context)
        {
            var controller = new PlacesController(context);
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
        public async Task Index_ReturnsAllPlaces()
        {
            using var context = CreateContext();
            SeedDatabase(context);
            var controller = CreateControllerWithSession(context);

            var result = await controller.Index(null) as ViewResult;

            var model = Assert.IsAssignableFrom<List<PlaceViewModel>>(result.Model);
            Assert.Equal(3, model.Count);
        }

        [Fact]
        public async Task Index_FiltersPlaces()
        {
            using var context = CreateContext();
            SeedDatabase(context);
            var controller = CreateControllerWithSession(context);

            var result = await controller.Index("Main Hall") as ViewResult;

            var model = Assert.IsAssignableFrom<List<PlaceViewModel>>(result.Model);
            Assert.Single(model);
            Assert.Equal("Main Hall", model.First().PlaceName);
        }

        [Fact]
        public async Task Details_ReturnsPlaceViewModel()
        {
            using var context = CreateContext();
            SeedDatabase(context);
            var controller = new PlacesController(context);

            var result = await controller.Details(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<PlaceViewModel>(viewResult.Model);
            Assert.Equal(1, model.PlaceID);
            Assert.Equal("Main Hall", model.PlaceName);
        }

        [Fact]
        public async Task Details_ReturnsNotFound()
        {
            using var context = CreateContext();
            SeedDatabase(context);
            var controller = new PlacesController(context);

            var result = await controller.Details(999);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Create_ReturnsViewResult()
        {
            using var context = CreateContext();
            var controller = new PlacesController(context);

            var result = controller.Create();

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Create_AddsNewPlace()
        {
            using var context = CreateContext();
            SeedDatabase(context);
            var controller = new PlacesController(context);
            var newPlace = new PlaceViewModel
            {
                PlaceName = "New Place",
                Geolocation = "50.999, 30.555"
            };

            var result = await controller.Create(newPlace);

            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(controller.Index), redirectToActionResult.ActionName);

            var place = context.Places.FirstOrDefault(p => p.PlaceName == "New Place");
            Assert.NotNull(place);
        }

        [Fact]
        public async Task Create_ReturnsViewWithModelError()
        {
            using var context = CreateContext();
            var controller = new PlacesController(context);

            controller.ModelState.AddModelError("Geolocation", "Required");

            var model = new PlaceViewModel
            {
                PlaceName = "Incomplete Place"
            };

            var result = await controller.Create(model) as ViewResult;

            Assert.NotNull(result);
            Assert.IsType<PlaceViewModel>(result.Model);
            Assert.False(controller.ModelState.IsValid);
        }

        [Fact]
        public async Task Edit_ReturnsPlaceViewModel()
        {
            using var context = CreateContext();
            SeedDatabase(context);
            var controller = new PlacesController(context);

            var result = await controller.Edit(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<PlaceViewModel>(viewResult.Model);
            Assert.Equal(1, model.PlaceID);
            Assert.Equal("Main Hall", model.PlaceName);
        }

        [Fact]
        public async Task Edit_UpdatesPlace()
        {
            using var context = CreateContext();
            SeedDatabase(context);
            var controller = new PlacesController(context);

            var updatedPlace = new PlaceViewModel
            {
                PlaceID = 1,
                PlaceName = "Updated Main Hall",
                Geolocation = "50.123, 30.567"
            };

            var result = await controller.Edit(updatedPlace);

            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(controller.Index), redirectToActionResult.ActionName);

            var place = await context.Places.FindAsync(1);
            Assert.Equal("Updated Main Hall", place.PlaceName);
        }

        [Fact]
        public async Task Edit_ReturnsNotFound()
        {
            using var context = CreateContext();
            SeedDatabase(context);
            var controller = new PlacesController(context);

            var nonExistentPlace = new PlaceViewModel
            {
                PlaceID = 999,
                PlaceName = "Non-Existent Place",
                Geolocation = "XX000000"
            };

            var result = await controller.Edit(nonExistentPlace);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_ReturnsNotFound()
        {
            using var context = CreateContext();
            SeedDatabase(context);
            var controller = new PlacesController(context);

            var result = await controller.Delete(999);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_RemovesPlace()
        {
            using var context = CreateContext();
            SeedDatabase(context);
            var controller = new PlacesController(context);

            var result = await controller.DeleteConfirmed(1);

            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(controller.Index), redirectToActionResult.ActionName);

            var place = context.Places.Find(1);
            Assert.Null(place);
        }

        [Fact]
        public void PlaceExists_ReturnsTrue()
        {
            using var context = CreateContext();
            SeedDatabase(context);
            var controller = new PlacesController(context);

            bool exists = controller.PlaceExists(1);

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

            var controller = new PlacesController(context)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = httpContextMock.Object
                }
            };

            var result = controller.ClearFilters() as RedirectToActionResult;

            cookiesMock.Verify(c => c.Delete("SearchPlace"), Times.Once);
            cookiesMock.Verify(c => c.Delete("Page"), Times.Once);

            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
        }
    }
}