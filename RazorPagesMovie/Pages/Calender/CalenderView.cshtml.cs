using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorPagesMovie.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RazorPagesMovie.Pages.Calender
{
    [Authorize]
    public class CalendarViewModel : PageModel
    {
        private readonly RazorPagesMovie.Data.RazorPagesMovieContext _context;

        public CalendarViewModel(RazorPagesMovie.Data.RazorPagesMovieContext context)
        {
            _context = context;
        }

        [BindProperty]
        public List<Room> Rooms { get; set; }
        public List<RoomReservationcs> Reservations { get; set; }
        public List<DateTime> DateRange { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // Lookup to hold reservation data
        public List<ReservationDetails> ReservationLookup { get; set; }

        public class ReservationDetails
        {
            public int RoomId { get; set; }
            public DateTime CheckInDate { get; set; }
            public DateTime CheckOutDate { get; set; }
            public int ReservationId { get; set; }
            public bool IsThirdPartyBooking { get; set; }
            public int? ThirdPartyHandlerId { get; set; } // Nullable since normal bookings won't have a handler
        }

        public async Task OnGetAsync(DateTime? startDate, DateTime? endDate)
        {
            StartDate = startDate ?? DateTime.Today;
            EndDate = endDate ?? StartDate.AddDays(29);

            DateRange = Enumerable.Range(0, (EndDate - StartDate).Days + 1)
                .Select(i => StartDate.AddDays(i)).ToList();

            Rooms = await _context.Room.ToListAsync();

            // Fetch reservations that overlap with the selected date range and join with Reservations table
            ReservationLookup = await _context.RoomReservationcs
            .Where(rr => rr.CheckInDate <= DateRange.Last()
                         && rr.CheckOutDate >= DateRange.First()
                         && rr.Status == 1)
            .Join(
                _context.Reservations, // Joining with Reservations table
                rr => rr.ResevationId,
                r => r.Id,
                (rr, r) => new { rr, r } // Temporary anonymous object to apply further filters
            )
            .Where(joined => joined.r.validity == 1) // Filtering on Validity = 1
            .Select(joined => new ReservationDetails
            {
                RoomId = joined.rr.RoomId,
                CheckInDate = joined.rr.CheckInDate,
                CheckOutDate = joined.rr.CheckOutDate,
                ReservationId = joined.rr.ResevationId,
                IsThirdPartyBooking = joined.r.IsThirdPartyBooking,
                ThirdPartyHandlerId = joined.r.ThirdPartyHandlerId // Fetching handler ID
            })
            .ToListAsync();

        }
    }
}
