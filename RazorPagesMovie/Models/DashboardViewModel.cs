namespace RazorPagesMovie.Models
{
    public class DashboardViewModel
    {
        public int TotalReservation { get; set; }
        public int UpcomingReservation { get; set; }
        public int ActiveReservation { get; set; }
        public int TotalRooms { get; set;}
        public int AvailableRoomCountToday { get; set; }
        public int IntendedCheckinCountToday { get; set; }
        public int IntendedCheckinCountTomorrow { get; set; }
        public int IntendedCheckOutCountToday { get; set; }
        public int IntendedCheckOutCountTomorrow { get; set; }



    }
}
