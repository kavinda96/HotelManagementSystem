﻿using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using RazorPagesMovie.Data;
using RazorPagesMovie.Models;
using RazorPagesMovie.Services;
using Microsoft.EntityFrameworkCore;

namespace RazorPagesMovie.Pages.Reserve
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly RazorPagesMovieContext _context;
        private readonly ReservationService _reservationService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CreateModel(RazorPagesMovieContext context,
                           ReservationService reservationService,
                           UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _reservationService = reservationService;
            _userManager = userManager;
        }

        public int InvoiceNumber { get; set; }
        [BindProperty]
        public Reservations Reservations { get; set; } = new Reservations();

        public IList<ThirdPartyHandlers> ThirdPartyHandlers { get; set; } = new List<ThirdPartyHandlers>();

        public IList<Room> Rooms { get; set; } = new List<Room>();

        public async Task<IActionResult> OnGetAsync()
        {
            ThirdPartyHandlers = await _context.ThirdPartyHandlers.ToListAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {          

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
           

            var user = await _userManager.GetUserAsync(User); // Get the currently logged-in user
            var secondaryUserId = user.SecondaryUserId.ToString(); // Get the SecondaryUserId


            Reservations.CheckInDate = DateTime.Parse(Request.Form["checkInDate"]);
            Reservations.ExpectedCheckOutDate = DateTime.Parse(Request.Form["checkOutDate"]);
            Reservations.SelectedRooms = string.Join(",", selectedRoomIdsList);
            Reservations.SelectedRoomsNos = selectedRoomNumbersString;
            Reservations.MasterbillId = Reservations.Id;
            Reservations.status = 0;
            Reservations.UserId = secondaryUserId; // Assign the user ID here

            _context.Reservations.Add(Reservations);
            await _context.SaveChangesAsync();

            foreach (var roomId in selectedRoomIdsList)
            {
               
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