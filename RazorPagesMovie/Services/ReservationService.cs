using RazorPagesMovie.Data;
using RazorPagesMovie.Models;
using System.Linq;

namespace RazorPagesMovie.Services
{
    public class ReservationService
    {

        private readonly RazorPagesMovieContext _dbContext;

        public ReservationService(RazorPagesMovieContext dbContext)
        {
            _dbContext = dbContext;
        }



       




        public List<Reservations> GetAllReservations()
        {
            return _dbContext.Reservations.ToList();
        }

        public Reservations GetReservationById(int reservationId)
        {
            return _dbContext.Reservations.FirstOrDefault(r => r.Id == reservationId);
        }
        public List<RoomReservationcs>  GetRoomReservationsById(int reservationId)
        {
            return _dbContext.RoomReservationcs
           .Where(r => r.ResevationId == reservationId)
           .ToList();
        }





        public void UpdateCheckoutDateForReservation(int reservationId, DateTime newCheckoutDate)
        {
            // Retrieve room reservations associated with the reservation ID
            var roomReservations = _dbContext.RoomReservationcs
                .Where(r => r.ResevationId == reservationId)
                .ToList();

            // Update the checkout date for each room reservation
            foreach (var reservation in roomReservations)
            {
                reservation.CheckOutDate = newCheckoutDate;
            }

            // Save changes to the database
            _dbContext.SaveChanges();
        }
        public void UpdateCusNameForReservation(int reservationId, string NewName)
        {
            // Retrieve room reservations associated with the reservation ID
            var roomReservations = _dbContext.Reservations
                .Where(r => r.Id == reservationId)
                .ToList();

            // Update the checkout date for each room reservation
            foreach (var reservation in roomReservations)
            {
                reservation.CustomerName = NewName;
            }

            // Save changes to the database
            _dbContext.SaveChanges();
        }

        public void UpdateRoomStatusWhenCheckout(int reservationId)
        {
            var roomReservations = _dbContext.RoomReservationcs
                           .Where(r => r.ResevationId == reservationId)
                           .ToList();

            // Update room availability for each room reservation
            foreach (var roomReservation in roomReservations)
            {
                var roomsToUpdate = _dbContext.Room
                    .Where(r => r.Id == roomReservation.RoomId)
                    .ToList();

                foreach (var room in roomsToUpdate)
                {
                    room.IsAvailable = 1; // Assuming 1 represents true for isAvailable
                }
            }

            // Save changes to the database
            _dbContext.SaveChanges();
        }



       




        public Room GetRoomDeatailById(int roomId)
        {
            return _dbContext.Room.FirstOrDefault(r => r.Id == roomId);
        }
    }
}
