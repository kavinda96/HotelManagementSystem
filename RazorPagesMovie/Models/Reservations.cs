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

        [Display(Name = "NIN")]
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

        [DataType(DataType.Date)]
        public DateTime? ExpectedCheckInDate { get; set; }
    }
}