using Booklir.Core.Interfaces;
using Booklir.Queries;
using Microsoft.AspNetCore.Mvc;

namespace Booklir.Controllers
{
    public class DestinationController : Controller
    {
        private readonly IDestinationService _destinationService;

        public DestinationController(IDestinationService destinationService)
        {
            _destinationService = destinationService;
        }

        public async Task<IActionResult> Index()
        {
            var result = await _destinationService.GetAllAsync(new DestiantionQueryParameters() { PageSize = int.MaxValue });
            return View(result);
        }

        public async Task<IActionResult> Details(int id)
        {
            var destination = await _destinationService.GetByIdAsync(id); 
            if (destination == null)
            {
                TempData["ErrorMessage"] = "Destination not found.";
                return RedirectToAction("Index");
            }
            return View(destination);
        }
    }
}
