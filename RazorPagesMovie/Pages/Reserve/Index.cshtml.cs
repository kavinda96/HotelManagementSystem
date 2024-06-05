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
    public class IndexModel : PageModel
    {
        private readonly RazorPagesMovie.Data.RazorPagesMovieContext _context;
        private readonly ReservationService _reservationService;

        public IndexModel(RazorPagesMovie.Data.RazorPagesMovieContext context, ReservationService reservationService)
        {
            _context = context;
            _reservationService = reservationService;
        }

        public IList<Reservations> Reservations { get;set; } = default!;

        public Reservations reservations { get;set; } = default!;
        public DateTime? startDate { get; set; }
        public DateTime? endDate { get; set; }

        public long? InvoiceNo { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }

        public async Task OnGetAsync()
        {
            //Reservations = _reservationService.GetAllReservations();
            Reservations = GetReservations(StartDate, EndDate);
        }

        public IActionResult OnGetBilling(int reservationId)
        {
            var reservation = _reservationService.GetReservationById(reservationId);
            if (reservation == null)
            {
                return NotFound();
            }

            return RedirectToPage("/Billing/Create", new
            {
                reservationId = reservation.Id,
                customerName = reservation.CustomerName,
                room = reservation.SelectedRoomsNos,
                checkInDate = reservation.CheckInDate.ToString("yyyy-MM-dd"),
                checkOutDate = reservation.ExpectedCheckOutDate.ToString("yyyy-MM-dd"),
                InvoiceNo = reservation.MasterbillId
            });
        }

        private List<Reservations> GetReservations(DateTime? startDate, DateTime? endDate)
        {
            var query = _context.Reservations.AsQueryable();

            if (startDate.HasValue)
            {
                query = query.Where(r => r.CheckInDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(r => r.CheckInDate <= endDate.Value);
            }

            return query.ToList();
        }

    }
}
