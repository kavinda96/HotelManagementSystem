using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RazorPagesMovie.Models;
using RazorPagesMovie.Services;
using System.Collections.Generic;
using System.Linq;

namespace RazorPagesMovie.Pages.Billing
{
    [Authorize]
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

        public IList<Food> FoodItems { get; set; } = new List<Food>();

        //public List<Bill> BillingTransactions { get; set; }

        [BindProperty]
        public Bill NewTransaction { get; set; }

        public decimal? TotalAmount { get; set; }
        public string? SelectedBillingCategory { get; set; }
        public decimal TotalRoomCharge { get;  set; }
        public decimal DiscountedTotal { get; set; } = 0;
        public decimal DiscountedPrice { get; set; } = 0;

        public decimal TotalWithoutDiscount { get; set; } = 0;



        public string? billrecDesc { get; set; }
        public IActionResult OnGet(int? id)
        {
            if (id.HasValue)
            {
                Reservation = _reservationService.GetReservationById(id.Value);

                if (Reservation == null)
                {
                    _logger.LogError($"Reservation with ID {id} not found.");
                    return NotFound($"Reservation with ID {id} not found.");
                }

                var invoice_no = Reservation.Id;
                BillingTransactions = _billingTransactionService.GetBillingTransactionsByReservationId(invoice_no);
                NewTransaction = new Bill { Id = id.Value };
            }

            FoodItems = _context.Food.Where(f => f.IsAvailable == 1).ToList();

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

        public IActionResult OnPostAddAdditionalCharges(string invoiceNo, string category, string item, decimal amount, int qty, string description)
        {
            _logger.LogInformation("Received invoiceNo: {InvoiceNo}, Category: {Category}, Item: {Item}, Amount: {Amount}, Qty: {Qty}, Description: {Description}",
                invoiceNo, category, item, amount, qty, description);

            if (string.IsNullOrEmpty(category)  || string.IsNullOrEmpty(invoiceNo))
            {
                return new JsonResult(new { success = false, error = "All fields are required." });
            }

            // Try to parse the invoice number
            if (!int.TryParse(invoiceNo, out int parsedInvoiceNo))
            {
                return new JsonResult(new { success = false, error = "Invalid invoice number." });
            }

            // Create a new billing transaction object
            var newTransaction = new Bill
            {
                Category = category,  
                BillingItem =item,
                ItemPrice = amount,
                ItemQty = qty,
                Description = description,
                InvoiceNo = parsedInvoiceNo, // Set the parsed invoice number
                createdDate = DateTime.Now
            };

            // Add the transaction to the database
            _context.BillingTransactions.Add(newTransaction);
            _context.SaveChanges();

            return new JsonResult(new { success = true });
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
            var reservationToUpdate = _reservationService.GetReservationById(sid);
            if (reservationToUpdate == null)
            {
                return NotFound();
            }
            reservationToUpdate.status = 2;

           // Reservation = _reservationService.GetReservationById(sid);
            TimeSpan difference = reservationToUpdate.ExpectedCheckOutDate.Date - reservationToUpdate.CheckInDate.Date;
            int datediff = (int)difference.TotalDays;
            string selectedRooms = reservationToUpdate.SelectedRoomsNos;

            if(!reservationToUpdate.IsThirdPartyBooking) {
                TotalRoomCharge = await _context.CalculateRoomChargeAsync(sid);
                billrecDesc = "Check out :Charges for " + datediff + " days stay at Room No " + selectedRooms;
            }
            else {
                TotalRoomCharge = 0;
                billrecDesc = "Check out : Third Party Party Booking";
            }
           

            var newTransaction = new Bill
            {
                InvoiceNo = sid,
                createdDate = DateTime.Now,
                Category = "3",
                Description = billrecDesc,
                ItemPrice = TotalRoomCharge,
                ItemQty = 1
            };

         
            _context.BillingTransactions.Add(newTransaction);
            _reservationService.UpdateRoomStatusWhenCheckoutIn(sid, 1);
            await _context.SaveChangesAsync();
            return RedirectToPage("./Index", new { id = sid });
        }






        public async Task<IActionResult> OnPostCheckInAsync(int sid)
        {
            var reservationToUpdate = _reservationService.GetReservationById(sid);

            if (reservationToUpdate == null)
            {
                return NotFound();
            }

            // Update the check-in date
            reservationToUpdate.CheckInDate = Reservation.CheckInDate;
            reservationToUpdate.status = 1; // Active when check-in

            string selectedRooms = reservationToUpdate.SelectedRoomsNos;

            var newTransaction = new Bill
            {
                InvoiceNo = sid,
                createdDate = DateTime.Now,
                Category = "3",
                Description = "Check In - rooms: " + selectedRooms + " On Date: " + Reservation.CheckInDate.ToString("yyyy-MM-dd")
            };

            _context.BillingTransactions.Add(newTransaction);
            _reservationService.UpdateRoomStatusWhenCheckoutIn(sid, 2);
            _reservationService.UpdateCheckInDateForReservation(reservationToUpdate.Id, Reservation.CheckInDate);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index", new { id = sid });
        }

        public async Task<IActionResult> OnPostEditCheckoutDateAsync(int sid)
        {
            //if (!ModelState.IsValid)
            //{
            //    return Page();
            //}

            var reservationToUpdate = _reservationService.GetReservationById(sid);

            if (reservationToUpdate == null)
            {
                return NotFound();
            }
            reservationToUpdate.ExpectedCheckOutDate = Reservation.ExpectedCheckOutDate;
            _reservationService.UpdateCheckoutDateForReservation(reservationToUpdate.Id, Reservation.ExpectedCheckOutDate);
        

            await _context.SaveChangesAsync();

            return RedirectToPage("./Index", new { id = sid });
        }


        public async Task<IActionResult> OnPostDiscountAsync(int sid)
        {
            var reservationToUpdate = _reservationService.GetReservationById(sid);

            if (reservationToUpdate == null)
            {
                return NotFound();
            }

            // Update the check-in date
            reservationToUpdate.DiscountRate = Reservation.DiscountRate;                    
            
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index", new { id = sid });
        }




        public async Task<JsonResult> OnGetFoodItems(string term)
        {
            try
            {
                var foodItems = await _context.Food
                                              .Where(f => f.FoodName.Contains(term))
                                              .Select(f => new { id = f.Id, foodName = f.FoodName, price = f.Price })
                                              .ToListAsync();

                return new JsonResult(foodItems);
            } 
            catch (Exception ex)
            {
                // Return a valid JSON error response
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        public async Task<JsonResult> OnGetBeverageItems(string term)
        {
            try
            {
                var beverageItems = await _context.Beverage
                                              .Where(f => f.BeverageName.Contains(term))
                                              .Select(f => new { id = f.Id, beverageName = f.BeverageName, price = f.Price })
                                              .ToListAsync();

                return new JsonResult(beverageItems);
            }
            catch (Exception ex)
            {
                // Return a valid JSON error response
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

     

        public async Task<IActionResult> OnPostBillFinalizedAsync(int ReservationId)
        {
          
            // Find the reservation in the database
            var reservationToUpdate = await _context.Reservations.FindAsync(ReservationId);

            if (reservationToUpdate == null)
            {
                return new JsonResult(new { success = false, message = "Reservation not found." });
            }

            // Update the reservation status to 'finalized'
            reservationToUpdate.status = 3; // 3 = finalized

            DiscountedTotal = await _context.CalculateDiscountedTotalAsync(reservationToUpdate.Id); // final total after discount
            reservationToUpdate.TotalFinalAmount = DiscountedTotal;
            

            TotalWithoutDiscount = await _context.CalculateTotalWithoutDiscountAsync(reservationToUpdate.Id); //  total without discount
            reservationToUpdate.TotalAmount = TotalWithoutDiscount;

            if (reservationToUpdate.DiscountRate > 0)
            {
                DiscountedPrice = await _context.CalculateDiscountedPriceAsync(reservationToUpdate.Id); //discounted price from total
                reservationToUpdate.DiscountedPrice = DiscountedPrice;

                var newTransaction = new Bill
                {
                    InvoiceNo = ReservationId,
                    createdDate = DateTime.Now,
                    Category = "3",
                    Description = "Discounted Price " + reservationToUpdate.DiscountRate + "%",
                    ItemPrice = DiscountedPrice,
                    ItemQty = 1
                };


                _context.BillingTransactions.Add(newTransaction);
            }


            // Save changes to the database
            await _context.SaveChangesAsync();

            // Return success response
            return new JsonResult(new { success = true, message = "Bill finalized successfully." });
        }
    }
}

public class UpdateCheckoutDateRequest
{
    public int reservationId { get; set; }
    public DateTime newCheckoutDate { get; set; }
}