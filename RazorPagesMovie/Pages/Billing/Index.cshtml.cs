using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using RazorPagesMovie.Models;
using RazorPagesMovie.Services;
using System.Collections.Generic;
using System.Linq;

namespace RazorPagesMovie.Pages.Billing
{
    public class IndexModel : PageModel
    {
        private readonly ReservationService _reservationService;
        private readonly BillingTransactionService _billingTransactionService;
        private readonly RazorPagesMovie.Data.RazorPagesMovieContext _context;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(RazorPagesMovie.Data.RazorPagesMovieContext context, ReservationService reservationService, BillingTransactionService billingTransactionService, ILogger<IndexModel> logger)
        {
            _context = context;
            _reservationService = reservationService;
            _billingTransactionService = billingTransactionService;
            _logger = logger;
        }


       [BindProperty]
        public Reservations Reservation { get; set; }
        public RoomReservationcs RoomReservation { get; set; }
        public List<Bill> BillingTransactions { get; set; }
        //public List<Bill> BillingTransactions { get; set; }

        [BindProperty]
        public Bill NewTransaction { get; set; }

        public decimal? TotalAmount { get; set; }
        public string? SelectedBillingCategory { get; set; }
        public decimal TotalRoomCharge { get;  set; }
        public IActionResult OnGet(int id)
        {
            Reservation = _reservationService.GetReservationById(id);
            //RoomReservation = _reservationService.GetRoomReservationsById(id);
            var invoice_no = Reservation.Id;
            BillingTransactions = _billingTransactionService.GetBillingTransactionsByReservationId(invoice_no);
            NewTransaction = new Bill { Id = id };
            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState)
                {
                    _logger.LogError("ModelState error: {Key} - {Value}", error.Key, string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage)));
                }
                return Page();
            }

            if (NewTransaction != null)
            {
                NewTransaction.InvoiceNo = Reservation?.Id;
                _context.BillingTransactions.Add(NewTransaction);
                _context.SaveChanges();
                return RedirectToPage("./Index", new { id = NewTransaction.InvoiceNo });
            }

            return RedirectToPage("./Index");
        }


        public async Task<IActionResult> OnPostDeletedddAsync(int id)
        {
            // Find the transaction by ID
            var transaction = await _context.BillingTransactions.FindAsync(id);

            if (transaction != null )//&& transaction.tranStatus == 1)
            {
                // Update its status to 2 (Deleted)
                transaction.tranStatus = 2;
                await _context.SaveChangesAsync();
                return RedirectToPage("./Index", new { id = transaction.InvoiceNo });
            }

            // Redirect to the same page to see the updated list
            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnPostCheckOutAsync(int sid)
        {
            Reservation = _reservationService.GetReservationById(sid);
            TimeSpan difference =Reservation.ExpectedCheckOutDate - Reservation.CheckInDate;
            int datediff = (int)difference.TotalDays;
            string selectedRooms = Reservation.SelectedRoomsNos;


            TotalRoomCharge = await _context.CalculateRoomChargeAsync(sid);
           

            var newTransaction = new Bill
            {
                InvoiceNo = sid,
                createdDate = DateTime.Now,
                Category = "3",
                Description = "Check out Initialized: Room Charges for " + datediff + " days stay at Room No " + selectedRooms,
                ItemPrice = TotalRoomCharge,
                ItemQty = 1
            };

         
            _context.BillingTransactions.Add(newTransaction);
            _reservationService.UpdateRoomStatusWhenCheckout(sid);
            await _context.SaveChangesAsync();
            return RedirectToPage("./Index", new { id = sid });
        }

        public async Task<IActionResult> OnPostEditCheckoutDateAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var reservationToUpdate = await _context.Reservations.FindAsync(Reservation.Id);

            if (reservationToUpdate == null)
            {
                return NotFound();
            }
            _reservationService.UpdateCheckoutDateForReservation(Reservation.Id, Reservation.ExpectedCheckOutDate);
            

            reservationToUpdate.ExpectedCheckOutDate = Reservation.ExpectedCheckOutDate;

            await _context.SaveChangesAsync();

            return RedirectToPage("./Index", new { id = Reservation.Id }); // Adjust the redirection as needed
        }






    }
}

public class UpdateCheckoutDateRequest
{
    public int reservationId { get; set; }
    public DateTime newCheckoutDate { get; set; }
}