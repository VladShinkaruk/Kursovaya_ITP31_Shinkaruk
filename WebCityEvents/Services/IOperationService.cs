using WebCityEvents.ViewModels;

namespace WebCityEvents.Services
{
    public interface IOperationService
    {
        HomeViewModel GetHomeViewModel(int numberRows = 10);
    }
}
