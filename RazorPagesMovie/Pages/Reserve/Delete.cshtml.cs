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

namespace RazorPagesMovie.Pages.Reserve
{
    [Authorize]
    public class DeleteModel : PageModel
    {
        private readonly RazorPagesMovie.Data.RazorPagesMovieContext _context;

        public DeleteModel(RazorPagesMovie.Data.RazorPagesMovieContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Reservations Reservations { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservations = await _context.Reservations.FirstOrDefaultAsync(m => m.Id == id);

            if (reservations == null)
            {
                return NotFound();
            }
            else
            {
                Reservations = reservations;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservations = await _context.Reservations.FindAsync(id);
            if (reservations != null)
            {
                

                reservations.validity = 2; // invalid status
               
            }

            var existingRoomReservations = await _context.RoomReservationcs
               .Where(rr => rr.ResevationId == id)
               .ToListAsync();

            foreach (var roomReservation in existingRoomReservations)
            {
                roomReservation.Status = 2; // invalidated old ones
            }
            await _context.SaveChangesAsync();
            return RedirectToPage("./Index");
       
        }
    }
}
