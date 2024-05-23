using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorPagesMovie.Data;
using RazorPagesMovie.Models;

namespace RazorPagesMovie.Pages.Reserve
{
    public class IndexModel : PageModel
    {
        private readonly RazorPagesMovie.Data.RazorPagesMovieContext _context;

        public IndexModel(RazorPagesMovie.Data.RazorPagesMovieContext context)
        {
            _context = context;
        }

        public IList<Reservations> Reservations { get;set; } = default!;
        public DateTime? startDate { get; set; }
        public DateTime? endDate { get; set; }


        public async Task OnGetAsync()
        {
            var rese = from m in _context.Reservations
                        select m;

            if (!string.IsNullOrEmpty(startDate.ToString()))
            {
                rese = rese.Where(r => r.CheckInDate >= startDate && r.CheckInDate <= endDate);
            }


            Reservations = await rese.ToListAsync();
        }

        //public async Task<IActionResult> OnGetGetReservationsAsync(DateTime startDate, DateTime endDate)
        //{
        //    Reservations = await _context.Reservations
        //        .Where(r => r.CheckInDate >= startDate && r.CheckInDate <= endDate)
        //        .ToListAsync();

        //    return Partial("_ReservationGridPartial", Reservations);
        //}

        public async Task<IActionResult> OnGetGetReservationsAsync(DateTime startDate, DateTime endDate)
        {
            // Retrieve reservations based on the provided date range
            Reservations = await _context.Reservations
                .Where(r => r.CheckInDate >= startDate && r.CheckInDate <= endDate)
                .ToListAsync();

            // Return a partial view with the filtered reservations
            //return Partial("_ReservationGridPartial", Reservations);

            // return Partial("_ReservationGridPartial", Reservations);this
            //return Page();
            return Partial("_ReservationGridPartial", this);

        }
    }
}
