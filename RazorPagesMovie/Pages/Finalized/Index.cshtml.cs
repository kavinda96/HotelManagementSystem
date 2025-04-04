using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorPagesMovie.Data;
using RazorPagesMovie.Models;
using RazorPagesMovie.Services;

namespace RazorPagesMovie.Pages.Finalized
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly RazorPagesMovieContext _context;
        private readonly PaginationService _paginationService;
        private readonly ReservationService _reservationService;

        public IndexModel(RazorPagesMovieContext context, PaginationService paginationService, ReservationService reservationService)
        {
            _context = context;
            _paginationService = paginationService;
            _reservationService = reservationService;
        }

        public IList<Reservations> Reservations { get; set; } = default!;
        public int PageSize { get; set; } = 10; // Number of records per page
        public int CurrentPage { get; set; } = 1; // Current page number
        public int TotalPages { get; set; } // Total number of pages
        public int TotalRecords { get; set; } // Total number of records
        public string PageTitle { get; set; } = "Finalized Reservations";

        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }

        public async Task OnGetAsync(DateTime? startDate, DateTime? endDate, int pageIndex = 1)
        {
            CurrentPage = pageIndex;
            StartDate = startDate;
            EndDate = endDate;

            var query = _context.Reservations.Where(r => r.validity == 1 && r.status ==3).AsQueryable();

            // Apply filters for upcoming or finalized reservations
         
            // Apply date range filter
            if (startDate.HasValue)
            {
                query = query.Where(r => r.CheckInDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(r => r.CheckInDate <= endDate.Value);
            }

            // Fetch total records count (before pagination)
            //TotalRecords = query.Count();

            // Fetch paginated reservations using the pagination service
            var paginatedResult = await _paginationService.GetPaginatedListAsync(query, pageIndex, PageSize);

            Reservations = paginatedResult.Items;
            TotalPages = paginatedResult.TotalPages;
            TotalRecords = paginatedResult.TotalRecords;

        }

        public async Task<IActionResult> OnGetThirdPartyHandlerNameAsync(int handlerId)
        {
            var handlerName = await _context.ThirdPartyHandlers
                                            .Where(h => h.Id == handlerId)
                                            .Select(h => h.CompanyName)
                                            .FirstOrDefaultAsync();
            if (handlerName == null)
            {
                return NotFound();
            }
            return new JsonResult(handlerName);
        }




    }
}
