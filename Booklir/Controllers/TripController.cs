using AutoMapper;
using Booklir.Core.Interfaces;
using Booklir.Core.Results;
using Booklir.Models;
using Booklir.Queries;
using Booklir.ViewModels.Trip;
using Microsoft.AspNetCore.Mvc;

namespace Booklir.Controllers
{
    public class TripController : Controller
    {
        private readonly ITripService _tripService;
        private readonly IDestinationService _destinationService;
        private readonly IMapper _mapper;

        public TripController(ITripService tripService, IDestinationService destinationService, IMapper mapper)
        {
            _tripService = tripService;
            _destinationService = destinationService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> Search(TripSearchFilters filters)
        {
            // 1) fetch filtered trips
            var result = await _tripService.SearchHomeTripAsync(filters);

            // 2) fetch any sidebar data
            var popularDest = await _destinationService.GetPopularDestinationAsync(5);

            // 3) map to VM
            var tripVMs = result.Items
                .Select(t => _mapper.Map<TripViewModel>(t))
                .ToList();

            var vm = new TripSearchViewModel
            {
                Filters = filters,
                Results = new PagedResult<TripViewModel>(
                             tripVMs,
                             result.TotalCount,
                             result.PageSize,
                             result.Page),
                Destinations = popularDest
            };

            return Request.Headers["X-Requested-With"] == "XMLHttpRequest"
                ? PartialView("_TripSearchResults", vm)    // AJAX
                : View(vm);                                // full page
        }

        public async Task<IActionResult> Featured()
        {
            var trips = await _tripService.GetFeaturedTripsAsync();
            var vm = _mapper.Map<IEnumerable<TripViewModel>>(trips);
            return View(vm);
        }

    }
}
