using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RazorPagesMovie.Data;
using RazorPagesMovie.Models;
using RazorPagesMovie.Services;

namespace RazorPagesMovie.Pages.Reserve
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly RazorPagesMovie.Data.RazorPagesMovieContext _context;
        private readonly ReservationService _reservationService;


        public EditModel(RazorPagesMovie.Data.RazorPagesMovieContext context, ReservationService reservationService)
        {
            _context = context;
            _reservationService = reservationService;
        }

        [BindProperty]
        public Reservations Reservations { get; set; } = default!;

        public IList<Room> Rooms { get; set; } = new List<Room>();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            try
            {
                Rooms = await _context.Room.Where(r => r.IsAvailable == 1).ToListAsync();
            }
            catch (Exception ex)
            {
                // Log the exception and handle as necessary
                ModelState.AddModelError(string.Empty, "Unable to load rooms.");
            }
            var reservations =  await _context.Reservations.FirstOrDefaultAsync(m => m.Id == id);
            if (reservations == null)
            {
                return NotFound();
            }
            Reservations = reservations;



            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var selectedRoomIds = Request.Form["SelectedRoomIds"];
            var selectedRoomNumbers = Request.Form["SelectedRoomIds"];

            if (selectedRoomIds.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "No rooms selected.");
                Rooms = await _context.Room.Where(r => r.IsAvailable == 1).ToListAsync();
                return Page();
            }

            Reservations.SelectedRooms = string.Join(",", selectedRoomIds);


            foreach (var roomId in selectedRoomIds)
            {
                var room = await _context.Room.FindAsync(int.Parse(roomId));
                if (room != null)
                {
                    room.IsAvailable = 0;
                    _context.Room.Update(room);
                }
            }
          
            _context.Attach(Reservations).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReservationsExists(Reservations.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnGetAvailableRoomsAsync(string checkInDate, string checkOutDate, string[] selectedRoomIds)
        {
            if (DateTime.TryParse(checkInDate, out DateTime parsedCheckInDate) &&
                DateTime.TryParse(checkOutDate, out DateTime parsedCheckOutDate))
            {
                // Get all available rooms
                var availableRooms = await _reservationService.GetAvailableRooms(parsedCheckInDate, parsedCheckOutDate);

                // Get previously selected rooms
                var selectedRooms = await _context.Room
                    .Where(r => selectedRoomIds.Contains(r.Id.ToString()))
                    .ToListAsync();

                // Add selected rooms to available rooms list
                if (selectedRooms != null && selectedRooms.Any())
                {
                    availableRooms = availableRooms.Union(selectedRooms).ToList(); // Combine both lists
                }

                if (availableRooms == null || !availableRooms.Any())
                {
                    return Content("<div class='alert alert-warning'>No rooms available for the selected dates.</div>", "text/html");
                }

                return Partial("_AvailableRoomsPartial", availableRooms);
            }
            else
            {
                return Content("<div class='alert alert-danger'>Invalid date format.</div>", "text/html");
            }
        }


        private bool ReservationsExists(int id)
        {
            return _context.Reservations.Any(e => e.Id == id);
        }
    }
}
