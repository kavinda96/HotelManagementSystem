using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorPagesMovie.Data;
using RazorPagesMovie.Models;
using RazorPagesMovie.Services;

namespace RazorPagesMovie.Pages.Reserve
{
    public class CreateModel : PageModel
    {
        private readonly RazorPagesMovieContext _context;
        private readonly InvoiceNoGenerator _invoiceNoGenerator;

        public CreateModel(RazorPagesMovieContext context, InvoiceNoGenerator invoiceNoGenerator)
        {
            _invoiceNoGenerator = invoiceNoGenerator;
            _context = context;
        }
        public int InvoiceNumber { get; set; }
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
            //var selectedRoomNumbers = Request.Form["SelectedRoomIds"];

            var selectedRoomIdsList = selectedRoomIds.ToString().Split(',').Select(int.Parse).ToList();

            // Fetch room numbers based on selected room IDs
            var selectedRooms = await _context.Room
                .Where(r => selectedRoomIdsList.Contains(r.Id))
                .ToListAsync();

            var selectedRoomNumbersString = string.Join(",", selectedRooms.Select(r => r.RoomNo));



            if (selectedRoomIds.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "No rooms selected.");
                Rooms = await _context.Room.Where(r => r.IsAvailable == 1).ToListAsync();
                return Page();
            }

            Reservations.SelectedRooms = string.Join(",", selectedRoomIds);
            Reservations.SelectedRoomsNos = selectedRoomNumbersString;
            Reservations.MasterbillId = Reservations.Id; // reservations table master bill id is reservation id itself


            _context.Reservations.Add(Reservations);
            await _context.SaveChangesAsync();

           
            foreach (var roomId in selectedRoomIds)
            {
                var room = await _context.Room.FindAsync(int.Parse(roomId));
                if (room != null)
                {
                    // Optionally, update the IsAvailable status of the selected rooms
                    room.IsAvailable = 0;
                    _context.Room.Update(room);
                }
                
                //adding records to RoomReservations table 
                var roomReservations = new RoomReservationcs
                {
                    ResevationId = Reservations.Id,
                    RoomId = int.Parse(roomId),
                    CheckInDate = Reservations.CheckInDate,
                    CheckOutDate = Reservations.ExpectedCheckOutDate
                   
                };
                _context.RoomReservationcs.Add(roomReservations);
                await _context.SaveChangesAsync();

            }

            return RedirectToPage("./Index");
        }
    }
}
