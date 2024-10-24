using System.ComponentModel.DataAnnotations;

namespace RazorPagesMovie.Models
{
    public class ReservationNotes
    {
        public int Id { get; set; }
        public int ReservationId { get; set; }

        [Display(Name = "Description")]
        public string? Description { get; set; }

        public int AddedUserId { get; set; }
    }
}
