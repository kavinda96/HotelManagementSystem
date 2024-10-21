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
        private readonly RazorPagesMovieContext _context;
        private readonly PaginationService _paginationService;
        private readonly ReservationService _reservationService;

        public IndexModel(RazorPagesMovieContext context, PaginationService paginationService, ReservationService reservationService)
        {
            _context = context;
            _paginationService = paginationService;
            _reservationService = reservationService;
        }

        public IList<ReservationViewModel> Reservations { get; set; } = default!;
        public int PageSize { get; set; } = 10; // Number of records per page
        public int CurrentPage { get; set; } = 1; // Current page number
        public int TotalPages { get; set; } // Total number of pages
        public int TotalRecords { get; set; } // Total number of records
        public string PageTitle { get; set; } = "All Reservations";

        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }

        public async Task OnGetAsync(DateTime? startDate, DateTime? endDate, int pageIndex = 1, bool showUpcoming = false, bool showFinalized = false, bool showTPB = false)
        {
            CurrentPage = pageIndex;
            StartDate = startDate;
            EndDate = endDate;

            var query = _context.Reservations.Where(r => r.validity == 1).AsQueryable();

            // Apply filters for upcoming or finalized reservations
            if (showUpcoming)
            {
                query = query.Where(r => r.CheckInDate >= DateTime.Now); // Upcoming: CheckInDate is in the future
                PageTitle = "Upcoming Reservations";
            }

            if (showFinalized)
            {
                query = query.Where(r => r.status == 3); // Finalized: status equals 3
                PageTitle = "Finalized Reservations";
            }

            if (showTPB)
            {
                query = query.Where(r => r.IsThirdPartyBooking == true);
                PageTitle = "Third Party Reservations";
            }

            // Apply date range filter
            if (startDate.HasValue)
            {
                query = query.Where(r => r.CheckInDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(r => r.CheckInDate <= endDate.Value);
            }

            // Fetch paginated reservations using the pagination service, projecting to the ViewModel
            var paginatedResult = await _paginationService.GetPaginatedListAsync(query
                .Select(r => new ReservationViewModel
                {
                    Id = r.Id,
                    CustomerName = r.CustomerName,
                    NIN = r.NIN,
                    Address = r.Address,
                    Country = r.Country,
                    Mobile = r.Mobile,
                    CheckInDate = r.CheckInDate,
                    ExpectedCheckOutDate = r.ExpectedCheckOutDate,
                    GuestCount = r.GuestCount,
                    RealCheckOutDate = r.RealCheckOutDate,
                    SelectedRooms = r.SelectedRooms,
                    SelectedRoomsNos = r.SelectedRoomsNos,
                    MasterbillId = r.MasterbillId,
                    Status = r.status,
                    DiscountRate = r.DiscountRate,
                    IsThirdPartyBooking = r.IsThirdPartyBooking,
                    CreatedDate = r.CreatedDate,
                    DiscountedPrice = r.DiscountedPrice,
                    TotalAmount = r.TotalAmount,
                    TotalFinalAmount = r.TotalFinalAmount,
                    ThirdPartyHandlerId = r.ThirdPartyHandlerId,
                    BookingReference = r.BookingReference,
                    SelectedCurrency = r.SelectedCurrency,
                    Email = r.Email,
                    Validity = r.validity,
                    UserId = r.UserId,
                    ThirdPartyHandlerName = r.IsThirdPartyBooking
                        ? _context.ThirdPartyHandlers
                            .Where(h => h.Id == r.ThirdPartyHandlerId)
                            .Select(h => h.CompanyName)
                            .FirstOrDefault()
                        : "N/A" // Or any default value for non-third party bookings
                }), pageIndex, PageSize);

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
