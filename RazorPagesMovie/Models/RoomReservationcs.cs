using System.ComponentModel.DataAnnotations;

namespace RazorPagesMovie.Models
{
    public class RoomReservationcs
    {
        public int Id { get; set; }     
        public int RoomId { get; set; }     
        public int ResevationId { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }

    }
}
//test 
//test 2