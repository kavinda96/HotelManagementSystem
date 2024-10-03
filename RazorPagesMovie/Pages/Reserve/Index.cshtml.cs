using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorPagesMovie.Data;
using RazorPagesMovie.Models;
using RazorPagesMovie.Services;

namespace RazorPagesMovie.Pages.Reserve
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly RazorPagesMovie.Data.RazorPagesMovieContext _context;
        private readonly ReservationService _reservationService;

        public IndexModel(RazorPagesMovie.Data.RazorPagesMovieContext context, ReservationService reservationService)
        {
            _context = context;
            _reservationService = reservationService;
        }

        public IList<Reservations> Reservations { get; set; } = default!;
        public int PageSize { get; set; } = 10; // Number of records per page
        public int CurrentPage { get; set; } = 1; // Current page number
        public int TotalPages { get; set; } // Total number of pages
        public int TotalRecords { get; set; } // Total number of records

        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }

        public string PageTitle { get; set; } = "All Reservations";

        public async Task OnGetAsync(int pageIndex = 1, bool showUpcoming = false, bool showFinalized = false)
        {
            CurrentPage = pageIndex;

            // Determine the total number of records based on filters (Upcoming or Finalized)
            var totalQuery = _context.Reservations.AsQueryable();

            if (showUpcoming)
            {
                totalQuery = totalQuery.Where(r => r.CheckInDate >= DateTime.Now);
            }
            if (showFinalized)
            {
                totalQuery = totalQuery.Where(r => r.status == 3);
            }

            TotalRecords = totalQuery.Count(); // Get the total number of filtered records
            TotalPages = (int)Math.Ceiling(TotalRecords / (double)PageSize);

            // Fetch paginated reservations based on the filter (Upcoming or Finalized)
            Reservations = GetReservations(StartDate, EndDate, pageIndex, showUpcoming, showFinalized);
        }

        private List<Reservations> GetReservations(DateTime? startDate, DateTime? endDate, int pageIndex, bool showUpcoming, bool showFinalized)
        {
            var query = _context.Reservations.AsQueryable();

            // Apply date range filter
            if (startDate.HasValue)
            {
                query = query.Where(r => r.CheckInDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(r => r.CheckInDate <= endDate.Value);
            }

            // Apply specific filters for upcoming or finalized reservations
            if (showUpcoming)
            {
                query = query.Where(r => r.CheckInDate >= DateTime.Now); // Upcoming: CheckInDate is in the future
            }

            if (showFinalized)
            {
                query = query.Where(r => r.status == 3); // Finalized: status equals 3
            }

            // Apply pagination
            return query
                .Skip((pageIndex - 1) * PageSize) // Skip records for previous pages
                .Take(PageSize) // Take records for the current page
                .ToList();
        }


    }
}
