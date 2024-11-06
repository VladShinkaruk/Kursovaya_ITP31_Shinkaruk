using Microsoft.AspNetCore.Mvc;
using WebCityEvents.Services;

namespace WebCityEvents.Controllers
{
    public class HomeController : Controller
    {
        private readonly IOperationService _operationService;
        private const int PageSize = 20;

        public HomeController(IOperationService operationService)
        {
            _operationService = operationService;
        }

        [HttpGet]
        [ResponseCache(Duration = 286, Location = ResponseCacheLocation.Client, NoStore = false)]
        public IActionResult Index(int numberRows = 10)
        {
            var model = _operationService.GetHomeViewModel(numberRows);
            return View(model);
        }
    }
}