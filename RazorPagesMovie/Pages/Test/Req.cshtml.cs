using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorPagesMovie.Models;
using RazorPagesMovie.Services;
using System.Threading.Tasks;

namespace RazorPagesMovie.Pages.Test
{
    [Authorize]
    [IgnoreAntiforgeryToken(Order = 1001)]
    public class ReqModel : PageModel
    {
        private readonly RazorPagesMovie.Data.RazorPagesMovieContext _dbContext;
        private readonly ReservationService _reservationService;

        public ReqModel (RazorPagesMovie.Data.RazorPagesMovieContext context, ReservationService reservationService)
        {
            _dbContext = context;
            _reservationService = reservationService;
        }


        [BindProperty]
        public int ReservationId { get; set; }

        [BindProperty]
        public string CurrentCustomerName { get; set; }

        [BindProperty]
        public string NewCustomerName { get; set; }

        [BindProperty]
        public DateTime OldCheckoutDate { get; set; }
        [BindProperty]
        public DateTime NewCheckoutDate { get; set; }


        public async Task<IActionResult> OnGetAsync(int id)
        {
            Console.WriteLine("get method");

            var reservation = await _dbContext.Reservations.FindAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }

            ReservationId = reservation.Id;
            CurrentCustomerName = reservation.CustomerName;
            OldCheckoutDate = reservation.ExpectedCheckOutDate;

            return Page();
        }

        //public int OnGetNameAsync(int id, string CurrentCustomerName = null, string NewCustomerName =null,int ReservationId=0)
        //{
        //    return ReservationId + 100;
        //}


        //[HttpPost]

        //public IActionResult OnPostUpdateCustomerNameAsync()
        //{
        //    Console.WriteLine("updatePost method");
        //    var name = Request.Form["ReservationId"];
        //    var email = Request.Form["NewCustomerName"];



        //    var reservation = _dbContext.Reservations.FindAsync(ReservationId);
        //    if (reservation == null)
        //    {
        //        return NotFound();
        //    }
        //    _reservationService.UpdateCusNameForReservation(ReservationId, NewCustomerName);
        //    _dbContext.SaveChangesAsync();
        //    return new JsonResult(new { success = true, message = "Customer name updated successfully." });

        //}

        [HttpPost]
        public async Task<IActionResult> OnPostUpdateCustomerNameAsync([FromBody] UpdateCustomerNameRequest request)
        {
            var reservation = await _dbContext.Reservations.FindAsync(request.ReservationId);
            if (reservation == null)
            {
                return NotFound();
            }

            reservation.CustomerName = request.NewCustomerName;
            reservation.ExpectedCheckOutDate = request.NewCheckoutDate;

            await _dbContext.SaveChangesAsync();

            return new JsonResult(new { success = true, message = "Customer name updated successfully.", newCustomerName = request.NewCustomerName, newCheckoutDate = request.NewCheckoutDate });
        }
        public class UpdateCustomerNameRequest
        {
            public int ReservationId { get; set; }
            public string NewCustomerName { get; set; }

            public DateTime NewCheckoutDate { get; set; } = DateTime.Now;
        }
    }
}
