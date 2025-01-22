using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly EmailService _emailService;
        private readonly IHotelInfoService _hotelInfoService;




        public IndexModel(RazorPagesMovie.Data.RazorPagesMovieContext context, ReservationService reservationService, BillingTransactionService billingTransactionService, ILogger<IndexModel> logger, UserManager<ApplicationUser> userManager, EmailService emailService, IHotelInfoService hotelInfoService)
        {
            _context = context;
            _reservationService = reservationService;
            _billingTransactionService = billingTransactionService;
            _logger = logger;
            _userManager = userManager;
            _emailService = emailService;
            _hotelInfoService = hotelInfoService;
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
        public decimal TotalRoomCharge { get; set; }
        public decimal DiscountedTotal { get; set; } = 0;
        public decimal DiscountedPrice { get; set; } = 0;

        public decimal TotalWithoutDiscount { get; set; } = 0;



        public string? billrecDesc { get; set; }
        public IActionResult OnGet(int? id)
        {
            if (id.HasValue)
            {
                // Retrieve the reservation
                Reservation = _reservationService.GetReservationById(id.Value);

                if (Reservation == null)
                {
                    _logger.LogError($"Reservation with ID {id} not found.");
                    return NotFound($"Reservation with ID {id} not found.");
                }

                // Get the invoice number
                var invoice_no = Reservation.Id;

                // Retrieve billing transactions for the reservation
                BillingTransactions = _billingTransactionService.GetBillingTransactionsByReservationId(invoice_no);
                NewTransaction = new Bill { Id = id.Value };

                // Retrieve the ThirdPartyHandler name using the ThirdPartyHandlerId from the reservation
                var handler = _context.ThirdPartyHandlers
                    .FirstOrDefault(h => h.Id == Reservation.ThirdPartyHandlerId);

                if (handler != null)
                {
                    ViewData["ThirdPartyHandlerName"] = handler.CompanyName;  // Passing the handler name to the view
                }
                else
                {
                    ViewData["ThirdPartyHandlerName"] = "No Handler Assigned"; // In case the handler is not found
                }

                // Get the current user synchronously
                var currentUser = _userManager.GetUserAsync(User).Result; // Synchronously wait for the task to complete
                var secondaryUserId = currentUser?.SecondaryUserId;

                ViewData["SecondaryUserId"] = secondaryUserId;
                ViewData["resId"] = invoice_no;

                ViewData["resStatus"] = Reservation.status;


            }

            // Retrieve available food items
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

            if (string.IsNullOrEmpty(category) || string.IsNullOrEmpty(invoiceNo))
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
                BillingItem = item,
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

            if (transaction != null)//&& transaction.tranStatus == 1)
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
            reservationToUpdate.status = 2; // Status update to Check out status

            // Reservation = _reservationService.GetReservationById(sid);
            TimeSpan difference = reservationToUpdate.ExpectedCheckOutDate.Date - reservationToUpdate.CheckInDate.Date;
            int datediff = (int)difference.TotalDays;
            string selectedRooms = reservationToUpdate.SelectedRoomsNos;

            if (!reservationToUpdate.IsThirdPartyBooking)
            {
                TotalRoomCharge = await _context.CalculateRoomChargeAsync(sid);
                //billrecDesc = "Check out :Charges for " + datediff + " days stay at Room No " + selectedRooms;
                billrecDesc = "Checkout:Charges for " + datediff + " days stay" ;
            }
            else
            {
                TotalRoomCharge = 0;
                billrecDesc = "Check out : 3rd Party Booking";
            }


            //below is to update room prices when check out in roomReservationcs table
            var roomReservations = _reservationService.GetRoomReservationsById(sid);

            if (roomReservations != null)
            {
                foreach (var roomRes in roomReservations)
                {
                    // Retrieve room details from the Rooms table
                    var room = await _context.Room.FindAsync(roomRes.RoomId);
                    if (room != null)
                    {
                        if (reservationToUpdate.SelectedCurrency == "USD")
                        {
                            roomRes.roomPrice = room.PriceUSD;
                        }
                        else
                        {
                            roomRes.roomPrice = room.Price;

                        }
                    
                        _context.RoomReservationcs.Update(roomRes); // Update each room reservation
                    }
                }
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



            if (reservationToUpdate.DiscountRate > 0 && !reservationToUpdate.IsThirdPartyBooking)
            {
                DiscountedPrice = await _context.CalculateDiscountedPriceAsync(reservationToUpdate.Id); //discounted price from total
                reservationToUpdate.DiscountedPrice = DiscountedPrice;

                var newTransaction2 = new Bill
                {
                    InvoiceNo = sid,
                    createdDate = DateTime.Now,
                    Category = "3",
                    Description = $"Discounted Price {reservationToUpdate.DiscountRate}%".Trim(),
                    ItemPrice = DiscountedPrice,
                    ItemQty = 1
                };


                _context.BillingTransactions.Add(newTransaction2);
            }
            else
            {
                reservationToUpdate.DiscountedPrice = 0;
            }
            //  _reservationService.UpdateRoomStatusWhenCheckoutIn(sid, 1);
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
           // reservationToUpdate.CheckInDate = Reservation.CheckInDate;
            reservationToUpdate.status = 1; // Status update to Checkin status

            string selectedRooms = reservationToUpdate.SelectedRoomsNos;

            var newTransaction = new Bill
            {
                InvoiceNo = sid,
                createdDate = DateTime.Now,
                Category = "3",
                //  Description = "Check In - rooms: " + selectedRooms + " On Date: " + Reservation.CheckInDate.ToString("yyyy-MM-dd")
                Description = "Check In : Date: " + reservationToUpdate.CheckInDate.ToString("yyyy-MM-dd")
            };

            _context.BillingTransactions.Add(newTransaction);
            //  _reservationService.UpdateRoomStatusWhenCheckoutIn(sid, 2);
            //_reservationService.UpdateCheckInDateForReservation(reservationToUpdate.Id, reservationToUpdate.CheckInDate);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index", new { id = sid });
        }

        public async Task<IActionResult> OnPostEditCheckoutDateAsync(int sid)
        {

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





        public async Task<JsonResult> OnGetFoodItems(string term, string currency)
        {
            try
            {
                var isNumeric = int.TryParse(term, out int foodCode);

                // Fetch the food items based on term and currency
                var foodItems = await _context.Food
                    .Where(f => isNumeric
                        ? f.FoodCode == foodCode && f.IsAvailable == 1 && f.FoodType == 1
                        : f.FoodName.Contains(term) && f.IsAvailable == 1 && f.FoodType == 1)
                    .Select(f => new
                    {
                        id = f.Id,
                        foodName = f.FoodName,
                        price = currency == "USD" ? f.PriceUSD : f.Price,  // Choose PriceUSD if currency is USD
                        foodCode = f.FoodCode
                    })
                    .ToListAsync();

                return new JsonResult(foodItems);
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }



        public async Task<JsonResult> OnGetBeverageItems(string term, string currency)
        {
            try
            {
                var isNumeric = int.TryParse(term, out int beveragecode);

                // Fetch the beverage items based on term and currency
                var beverageItems = await _context.Food
                    .Where(f => (isNumeric
                        ? f.FoodCode == beveragecode && f.IsAvailable == 1 && f.FoodType == 2
                        : f.FoodName.Contains(term) && f.IsAvailable == 1 && f.FoodType == 2))
                   .Select(f => new
                   {
                       id = f.Id,
                       beverageName = f.FoodName,
                       price = currency == "USD" ? f.PriceUSD : f.Price,  // Choose PriceUSD if currency is USD
                       beverageCode = f.FoodCode
                   })
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

            // Check if reservation is already finalized to prevent duplicate finalization
            if (reservationToUpdate.status == 3)
            {
                return new JsonResult(new { success = false, message = "Reservation is already finalized." });
            }

            // Update the reservation status to 'finalized'
            reservationToUpdate.status = 3; // 3 = finalized

            // Calculate the total amounts
            TotalWithoutDiscount = await _context.CalculateTotalWithoutDiscountAsync(reservationToUpdate.Id); // total without discount
            reservationToUpdate.TotalAmount = TotalWithoutDiscount;

            if (!reservationToUpdate.IsThirdPartyBooking)
            {
                DiscountedTotal = await _context.CalculateDiscountedTotalAsync(reservationToUpdate.Id); // final total after discount
                reservationToUpdate.TotalFinalAmount = DiscountedTotal;
            }
            else
            {
                reservationToUpdate.TotalFinalAmount = TotalWithoutDiscount;
            }

            // Save changes to the database
            await _context.SaveChangesAsync();

           

            // Return success response
            return new JsonResult(new { success = true, message = "Bill finalized." });
        }








        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostSendEmailWithPDF(IFormFile pdf, int reservationId)
        {
            try
            {
                // Retrieve reservation data
                var reservationToUpdate = await _context.Reservations.FindAsync(reservationId);
                if (reservationToUpdate == null)
                {
                    _logger.LogWarning("Reservation not found for ID: {ReservationId}", reservationId);
                    return new JsonResult(new { success = false, message = "Reservation not found." });
                }

                // Validate the PDF file
                if (pdf == null || !pdf.ContentType.Equals("application/pdf"))
                {
                    _logger.LogWarning("Invalid or missing PDF file. ContentType: {ContentType}", pdf?.ContentType);
                    return new JsonResult(new { success = false, message = "Invalid PDF file." });
                }

                // Update reservation status and calculate final amount if necessary
               


                // Convert PDF IFormFile to byte array for email attachment
                byte[] pdfBytes;
                using (var memoryStream = new MemoryStream())
                {
                    await pdf.CopyToAsync(memoryStream);
                    pdfBytes = memoryStream.ToArray();
                }


                DateTime currentDateTime = DateTime.Now;

                // Format the date-time as yyyymmddhhmm
                string formattedDateTime = currentDateTime.ToString("yyyyMMddHHmm");

                string subject = $"Your Bill has been Finalized : {formattedDateTime} - {reservationId}";



                // Define the email body content
                var toEmail = reservationToUpdate.Email;
                var hotelName = await _hotelInfoService.GetHotelNameAsync();
                var hotelContact = await _hotelInfoService.GetHotelContactAsync();
                var hotelAddress1 = await _hotelInfoService.GetHotelAddressline1Async();
                var hotelAddress2 = await _hotelInfoService.GetHotelAddressline2Async();
                var hotelEmail = await _hotelInfoService.GetHotelEmailAsync();
                var customerName = reservationToUpdate.CustomerName;
                var totalAmount = reservationToUpdate.TotalFinalAmount.ToString("N2");
                var currency = reservationToUpdate.SelectedCurrency;

                // Updated email body with dynamic data
                var bodyEmail = GetBillFinalizationEmailBody(hotelName, hotelAddress1, hotelAddress2, hotelContact, hotelEmail, customerName, reservationId, currency, totalAmount);

                // Send email with the PDF attached
                try
                {
                    string attachmentFileName = $"invoice_{reservationId}.pdf";

                    // var toEmail = reservationToUpdate.Email;
                    var emailSettings = await _hotelInfoService.GetEmailSettingsAsync();
                    await _emailService.SendEmailWithAttachmentAsync(
                        toEmail,
                        subject,
                        bodyEmail,
                        pdfBytes,
                       attachmentFileName
                    );

                    return new JsonResult(new { success = true, message = "Email sent successfully." });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending email with PDF attachment to {Email}", reservationToUpdate.Email);
                    return new JsonResult(new { success = false, message = "Error sending email." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error handling the request.");
                return new JsonResult(new { success = false, message = "Server error." });
            }
        }





  private string GetBillFinalizationEmailBody(
    string hotelName,
    string hotelAddress1,
    string hotelAddress2,
    string hotelContact,
    string hotelEmail,
    string customerName,
    int reservationId,
    string currency,
    string totalAmount)
        {
            return $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Bill Finalization</title>
    <style>
        body {{
            font-family: Arial, sans-serif;
            color: #333;
            background-color: #f7f7f7;
            padding: 20px;
        }}
        .container {{
            max-width: 600px;
            margin: 0 auto;
            background-color: #ffffff;
            border-radius: 8px;
            overflow: hidden;
            box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
        }}
        .header {{
            background-color: #004080;
            color: #ffffff;
            padding: 20px;
            text-align: center;
        }}
        .header h1 {{
            margin: 0;
            font-size: 24px;
        }}
        .content {{
            padding: 20px;
        }}
        .content h2 {{
            font-size: 20px;
            color: #004080;
        }}
        .content p {{
            line-height: 1.6;
            font-size: 16px;
        }}
        .footer {{
            background-color: #f1f1f1;
            padding: 10px;
            text-align: center;
            font-size: 12px;
            color: #666;
        }}
        .footer p {{
            margin: 5px 0;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>{hotelName}</h1>
            <p>{hotelAddress1}, {hotelAddress2}</p>
            <p>Contact: {hotelContact} | Email: {hotelEmail}</p>
        </div>
        <div class='content'>
            <h2>Dear {customerName},</h2>
            <p>We are pleased to inform you that your bill for reservation #{reservationId} has been finalized.</p>
            <p><strong>Total Amount Due: {currency} {totalAmount:N2}</strong></p>
            <p>Thank you for choosing {hotelName}. We hope you had a pleasant stay and look forward to welcoming you back in the future.</p>
            <p>Should you have any questions, please do not hesitate to reach out to us.</p>
            <br>
            <p>Best Regards,</p>
            <p>The {hotelName} Team</p>
        </div>
        <div class='footer'>
            <p>&copy; {DateTime.Now.Year} {hotelName}. All rights reserved.</p>
            <p>{hotelAddress1}, {hotelAddress2}</p>
        </div>
    </div>
</body>
</html>";
        }





        public async Task<IActionResult> OnPostAddNoteAsync(string Description, int ReservationId, int AddedUserId)
        {
          
           // Reservation = _reservationService.GetReservationById(ReservationId);
            var note = new ReservationNotes
            {
                ReservationId = ReservationId,
                Description = Description,
                AddedUserId = AddedUserId,
                CreatedDate = DateTime.UtcNow
            };

            _context.ReservationNotes.Add(note);
            await _context.SaveChangesAsync();

            return new JsonResult(new { success = true });
        }

        


        public async Task<IActionResult> OnGetNotesAsync(int reservationId)
        {
           

            try
            {
                var notes = await _context.ReservationNotes
                    .Where(n => n.ReservationId == reservationId)
                    .OrderByDescending(n => n.Id)
                    .ToListAsync();

             //   _logger.LogInformation($"*********************Found {notes.Count} notes for Reservation ID: {reservationId}");

                if (notes == null || !notes.Any())
                {
                   // _logger.LogWarning($"No notes found for Reservation ID: {reservationId}");
                    return Partial("_NotesListPartial", new List<RazorPagesMovie.Models.ReservationNotes>());
                }

                // Return the notes as a partial view
                return Partial("_NotesListPartial", notes);
            }
            catch (Exception ex)
            {
               // _logger.LogError(ex, $"Error fetching notes for Reservation ID: {reservationId}");
                return Partial("_Error"); // Create a partial view for error handling
            }
        }


    }
}

public class UpdateCheckoutDateRequest
{
    public int reservationId { get; set; }
    public DateTime newCheckoutDate { get; set; }
}