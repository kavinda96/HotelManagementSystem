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
        public DateTime EndDate { get; set; }  // New EndDate property

        // OnGet method to fetch room and reservation data
        public async Task OnGetAsync(DateTime? startDate, DateTime? endDate)
        {
            // Set default start date to today if not provided
            StartDate = startDate ?? DateTime.Today;

            // Set default end date to one month from start date if not provided
            EndDate = endDate ?? StartDate.AddDays(29);

            // Create a date range based on the start and end date
            DateRange = Enumerable.Range(0, (EndDate - StartDate).Days + 1).Select(i => StartDate.AddDays(i)).ToList();

            // Fetch all rooms
            Rooms = await _context.Room.ToListAsync();

            // Fetch reservations that overlap with the selected date range
            Reservations = await _context.RoomReservationcs
                .Where(r => r.CheckInDate <= DateRange.Last() && r.CheckOutDate >= DateRange.First() && r.Status == 1)
                .ToListAsync();
        }
    }
}
