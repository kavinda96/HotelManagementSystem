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
    public class CreateModel : PageModel
    {
        private readonly RazorPagesMovieContext _context;
        private readonly ReservationService _reservationService;

        public CreateModel(RazorPagesMovieContext context, ReservationService reservationService)
        {
            _context = context;
            _reservationService = reservationService;
        }

        public int InvoiceNumber { get; set; }
        [BindProperty]
        public Reservations Reservations { get; set; } = new Reservations();

        public IList<Room> Rooms { get; set; } = new List<Room>();

        //public async Task<IActionResult> OnGetAsync()
        //{
        //    try
        //    {
        //        Rooms = await _context.Room.Where(r => r.IsAvailable == 1).ToListAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        ModelState.AddModelError(string.Empty, "Unable to load rooms.");
        //    }
        //    return Page();
        //}

        public async Task<IActionResult> OnPostAsync()
        {
            //if (!ModelState.IsValid)
            //{
            //    Rooms = await _context.Room.Where(r => r.IsAvailable == 1).ToListAsync();
            //    return Page();
            //}

            var selectedRoomIds = Request.Form["SelectedRoomIds"];
            if (selectedRoomIds.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "No rooms selected.");
                // Rooms = await _context.Room.Where(r => r.IsAvailable == 1).ToListAsync();
                return Page();
            }
            var selectedRoomIdsList = selectedRoomIds.ToString().Split(',').Select(int.Parse).ToList();

            var selectedRooms = await _context.Room
                .Where(r => selectedRoomIdsList.Contains(r.Id))
                .ToListAsync();
           

            var selectedRoomNumbersString = string.Join(",", selectedRooms.Select(r => r.RoomNo));

           


            Reservations.CheckInDate = DateTime.Parse(Request.Form["checkInDate"]);
            Reservations.ExpectedCheckOutDate = DateTime.Parse(Request.Form["checkOutDate"]);
            Reservations.SelectedRooms = string.Join(",", selectedRoomIdsList);
            Reservations.SelectedRoomsNos = selectedRoomNumbersString;
            Reservations.MasterbillId = Reservations.Id;

            _context.Reservations.Add(Reservations);
            await _context.SaveChangesAsync();

            foreach (var roomId in selectedRoomIdsList)
            {
                var room = await _context.Room.FindAsync(roomId);
                if (room != null)
                {
                    room.IsAvailable = 0;
                    _context.Room.Update(room);
                }

                var roomReservations = new RoomReservationcs
                {
                    ResevationId = Reservations.Id,
                    RoomId = roomId,
                    CheckInDate = Reservations.CheckInDate,
                    CheckOutDate = Reservations.ExpectedCheckOutDate,
                    Status = 1 //active

                };
                _context.RoomReservationcs.Add(roomReservations);
            }

            await _context.SaveChangesAsync();
            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnGetAvailableRoomsAsync(string checkInDate, string checkOutDate)
        {
            if (DateTime.TryParse(checkInDate, out DateTime parsedCheckInDate) &&
                DateTime.TryParse(checkOutDate, out DateTime parsedCheckOutDate))
            {
                var availableRooms = await _reservationService.GetAvailableRooms(parsedCheckInDate, parsedCheckOutDate);

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


        public async Task<List<Room>> GetAvailableRooms(DateTime checkInDate, DateTime checkOutDate)
        {
            var availableRooms = await _context.Room
                .Where(room => !_context.RoomReservationcs
                    .Any(reservation =>
                        reservation.RoomId == room.Id &&
                        reservation.CheckInDate <= checkOutDate &&
                        reservation.CheckOutDate >= checkInDate))
                .ToListAsync();

            return availableRooms;
        }
    }
}