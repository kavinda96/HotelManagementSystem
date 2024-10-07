using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorPagesMovie.Data;
using RazorPagesMovie.Services;

namespace RazorPagesMovie.Pages.Availability
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ReservationService _reservationService;

        public IndexModel( ReservationService reservationService)
        {
          
            _reservationService = reservationService;
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

                // Group rooms by room type
                var roomsByType = availableRooms.GroupBy(r => r.RoomType)
                                                .ToDictionary(g => g.Key, g => g.ToList());

                return Partial("_availabilityPartial", roomsByType);
            }
            else
            {
                return Content("<div class='alert alert-danger'>Invalid date format.</div>", "text/html");
            }
        }

    }
}
