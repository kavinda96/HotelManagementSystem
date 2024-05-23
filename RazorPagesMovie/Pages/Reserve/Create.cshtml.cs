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
    public class CreateModel : PageModel
    {
        private readonly RazorPagesMovieContext _context;

        public CreateModel(RazorPagesMovieContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Reservations Reservations { get; set; } = new Reservations();

        public IList<Room> Rooms { get; set; } = new List<Room>();

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                Rooms = await _context.Room.Where(r => r.IsAvailable == 1).ToListAsync();
            }
            catch (Exception ex)
            {
                // Log the exception and handle as necessary
                ModelState.AddModelError(string.Empty, "Unable to load rooms.");
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                try
                {
                    Rooms = await _context.Room.Where(r => r.IsAvailable == 1).ToListAsync();
                }
                catch (Exception ex)
                {
                    // Log the exception and handle as necessary
                    ModelState.AddModelError(string.Empty, "Unable to load rooms.");
                }

                // Log validation errors
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        // This will log the errors to the console
                        Console.WriteLine($"Error in {state.Key}: {error.ErrorMessage}");
                    }
                }

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




            //int lastCustomer = _context.Reservations.OrderByDescending(c => c.Id).FirstOrDefault();
            //int nextId = (lastCustomer != null ? lastCustomer.MasterbillId.t : 0) + 1;
            //Customer.MainBillId = int.Parse("100" + nextId.ToString());



            _context.Reservations.Add(Reservations);
            await _context.SaveChangesAsync();

            // Optionally, update the IsAvailable status of the selected rooms
            foreach (var roomId in selectedRoomIds)
            {
                var room = await _context.Room.FindAsync(int.Parse(roomId));
                if (room != null)
                {
                    room.IsAvailable = 0;
                    _context.Room.Update(room);
                }
            }
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
