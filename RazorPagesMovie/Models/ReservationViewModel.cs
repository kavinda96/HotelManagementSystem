using System.ComponentModel.DataAnnotations;

namespace RazorPagesMovie.Models
{
    public class ReservationViewModel
    {
        public int Id { get; set; }
        public string CustomerName { get; set; }
        public string NIN { get; set; }
        public string Address { get; set; }
        public string Country { get; set; }
        public string Mobile { get; set; }

        [DataType(DataType.Date)]
        public DateTime CheckInDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime ExpectedCheckOutDate { get; set; }
        public int GuestCount { get; set; }
        public DateTime RealCheckOutDate { get; set; }
        public string? SelectedRooms { get; set; }
        public string? SelectedRoomsNos { get; set; }
        public long? MasterbillId { get; set; }
        public int Status { get; set; }
        public decimal DiscountRate { get; set; }
        public bool IsThirdPartyBooking { get; set; }
        public DateTime CreatedDate { get; set; }
        public decimal DiscountedPrice { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalFinalAmount { get; set; }
        public int? ThirdPartyHandlerId { get; set; }
        public string? BookingReference { get; set; }
        public string? SelectedCurrency { get; set; }
        public string? Email { get; set; }
        public int Validity { get; set; }
        public string? UserId { get; set; }
        public string? ThirdPartyHandlerName { get; set; } // Added for the handler name
    }
}
