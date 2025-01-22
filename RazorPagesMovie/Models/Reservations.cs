using System;
using System.ComponentModel.DataAnnotations;

namespace RazorPagesMovie.Models
{
    public class Reservations
    {
        public int Id { get; set; }

        [Display(Name = "Name")]
        [Required]
        [StringLength(100)]
        public string CustomerName { get; set; }

        [Display(Name = "NIC/Passport")]
        [Required]
        [StringLength(20)]
        public string NIN { get; set; }


        [Required]
        [StringLength(200)]
        public string Address { get; set; }

        [Required]
        [StringLength(100)]
        public string Country { get; set; }

        [Display(Name = "Contact No.")]
        [Required]
        [Phone]
        public string Mobile { get; set; }

        [Display(Name = "Check-in Date")]
        [Required]
        [DataType(DataType.Date)]
        public DateTime CheckInDate { get; set; } = DateTime.Now;

        [Display(Name = "Check-out Date")]
        [Required]
        [DataType(DataType.Date)]
        public DateTime ExpectedCheckOutDate { get; set; } = DateTime.Now;

        [Display(Name = "Guest Count")]
        [Required]
        [Range(1, 10)]
        public int GuestCount { get; set; }

        [DataType(DataType.Date)]
        public DateTime RealCheckOutDate { get; set; }

        [Display(Name = "Selected Rooms")]
        public string? SelectedRooms { get; set; }

        public string? SelectedRoomsNos { get; set; }
        public long? MasterbillId { get; set; }

        public int status { get; set; }



        public decimal DiscountRate { get; set; } = 0;

        public bool IsThirdPartyBooking { get; set; } = false;

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        //
        public decimal DiscountedPrice { get; set; } = 0;

        public decimal TotalAmount { get; set; } = 0;

        public decimal TotalFinalAmount { get; set; } = 0;

        public int? ThirdPartyHandlerId { get; set; }
        public string? BookingReference { get; set; }

        public string? SelectedCurrency { get; set; } 

        
        [EmailAddress]
        [Display(Name = "Email")]
        public string? Email {  get; set; }


        public int validity { get; set; } = 1; // 1= valid, 2 = invalid

        public string? UserId { get; set;}

     //   public DateTime UpdatedDate { get; set; } = DateTime.Now;

    }
}