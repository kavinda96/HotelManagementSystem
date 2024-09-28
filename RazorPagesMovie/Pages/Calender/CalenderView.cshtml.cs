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

        // OnGet method to fetch room and reservation data
        public async Task OnGetAsync(DateTime? startDate)
        {
            // Set default start date to today if not provided
            StartDate = startDate ?? DateTime.Today;

            // Create a 30-day date range starting from StartDate
            DateRange = Enumerable.Range(0, 30).Select(i => StartDate.AddDays(i)).ToList();

            // Fetch all rooms
            Rooms = await _context.Room.ToListAsync();

            // Fetch reservations that overlap with the selected date range
            Reservations = await _context.RoomReservationcs
                .Where(r => r.CheckInDate <= DateRange.Last() && r.CheckInDate >= DateRange.First())
                .ToListAsync();
        }
    }
}
