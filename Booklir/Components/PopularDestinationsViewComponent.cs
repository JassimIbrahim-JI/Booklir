using Booklir.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Booklir.Components
{
    public class PopularDestinationsViewComponent : ViewComponent
    {
        private readonly IDestinationService _destService;

        public PopularDestinationsViewComponent(IDestinationService destService)
        {
            _destService = destService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var dests = await _destService.GetPopularDestinationAsync(5);
            return View(dests);
        }
    }
}
