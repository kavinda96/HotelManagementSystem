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
//using RazorPagesMovie.Migrations;
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

        public IList<ThirdPartyHandlers> ThirdPartyHandlers { get; set; } = new List<ThirdPartyHandlers>();

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

            ThirdPartyHandlers = await _context.ThirdPartyHandlers.ToListAsync();

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

            if (selectedRoomIds.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "No rooms selected.");
                Rooms = await _context.Room.Where(r => r.IsAvailable == 1).ToListAsync();
                return Page();
            }

            // Update reservation with the selected rooms


            var selectedRoomIdss = selectedRoomIds.ToString().Split(',').Select(int.Parse).ToList();
            var selectedRooms = await _context.Room
              .Where(r => selectedRoomIdss.Contains(r.Id))
              .ToListAsync();

            Reservations.SelectedRooms = string.Join(",", selectedRoomIdss);

            var selectedRoomNumbersString = string.Join(",", selectedRooms.Select(r => r.RoomNo));

            Reservations.SelectedRoomsNos = selectedRoomNumbersString;
            Reservations.CheckInDate = DateTime.Parse(Request.Form["checkInDate"]);
            Reservations.ExpectedCheckOutDate = DateTime.Parse(Request.Form["checkOutDate"]);


            _context.Attach(Reservations).State = EntityState.Modified;

            // Update existing room reservations status to 2
            var existingRoomReservations = await _context.RoomReservationcs
                .Where(rr => rr.ResevationId == Reservations.Id)
                .ToListAsync();

            foreach (var roomReservation in existingRoomReservations)
            {
                roomReservation.Status = 2; // invalidated old ones
            }

            // Create new room reservations with status = 1
            foreach (var roomId in selectedRoomIdss)
            {
                var roomReservation = new RoomReservationcs
                {
                    ResevationId = Reservations.Id,
                    RoomId = roomId,
                    CheckInDate = Reservations.CheckInDate,
                    CheckOutDate = Reservations.ExpectedCheckOutDate,
                    Status = 1 // active
                };
                _context.RoomReservationcs.Add(roomReservation);
            }

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

        public async Task<IActionResult> OnGetAvailableRoomsAsync(string checkInDate, string checkOutDate, string[] selectedRoomIds, int reservationId)
        {
            if (DateTime.TryParse(checkInDate, out DateTime parsedCheckInDate) &&
                DateTime.TryParse(checkOutDate, out DateTime parsedCheckOutDate))
            {
                // Initialize the availableRooms list
                var availableRooms = await _reservationService.GetAvailableRooms(parsedCheckInDate, parsedCheckOutDate) ?? new List<Room>();

                // Get rooms that are currently selected (from the reservation)
                var selectedRoomIdsFromDb = await _context.RoomReservationcs
                 .Where(r => r.ResevationId == reservationId && r.Status == 1) // Add status active =1
                 .Select(r => r.RoomId)
                 .ToListAsync() ?? new List<int>();


                // Fetch the actual Room objects based on the Room IDs
                var selectedRoomsFromDb = await _context.Room
                    .Where(room => selectedRoomIdsFromDb.Contains(room.Id))
                    .ToListAsync() ?? new List<Room>();

                //// Mark the rooms as selected if they are already selected in the reservation or passed from client
                //foreach (var room in availableRooms)
                //{
                //    if (selectedRoomsFromDb.Any(r => r.Id == room.Id) || (selectedRoomIds != null && selectedRoomIds.Contains(room.Id.ToString())))
                //    {
                //        room.IsSelected = true;  // Mark room as selected
                //    }
                //}


                foreach (var room in selectedRoomsFromDb)
                {
                   
                        room.IsSelected = true;  // Mark room as selected
                    
                }



                // Append selected rooms from the DB that might not be in available rooms
                availableRooms = availableRooms.Union(selectedRoomsFromDb).ToList();

                // Check if any rooms are available to return
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
