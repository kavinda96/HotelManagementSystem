using Microsoft.EntityFrameworkCore;
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
                .Where(r => r.ResevationId == reservationId && r.Status == 1)
                .ToList();

            // Update the checkout date for each room reservation
            foreach (var reservation in roomReservations)
            {
                reservation.CheckOutDate = newCheckoutDate;
            }

            // Save changes to the database
            _dbContext.SaveChanges();
        }

        public void UpdateCheckInDateForReservation(int reservationId, DateTime newCheckInDate)
        {
            // Retrieve room reservations associated with the reservation ID
            var roomReservations = _dbContext.RoomReservationcs
                .Where(r => r.ResevationId == reservationId && r.Status == 1)
                .ToList();

            // Update the checkout date for each room reservation
            foreach (var reservation in roomReservations)
            {
                reservation.CheckInDate = newCheckInDate;
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

        public void UpdateRoomStatusWhenCheckoutIn(int reservationId, int action)
        {




            var roomReservations = _dbContext.RoomReservationcs
                           .Where(r => r.ResevationId == reservationId)
                           .ToList();


            if (action == 1) //action 1 = checkIn
            {
                // Update room availability for each room reservation
                foreach (var roomReservation in roomReservations)
                {
                    var roomsToUpdate = _dbContext.Room
                        .Where(r => r.Id == roomReservation.RoomId)
                        .ToList();

                    foreach (var room in roomsToUpdate)
                    {
                        room.IsAvailable = 0; // Assuming 1 represents true for isAvailable
                    }
                }
            }

            else { // action 2 = checkout
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
            }

            // Save changes to the database
            _dbContext.SaveChanges();
        }



       




        public Room GetRoomDeatailById(int roomId)
        {
            return _dbContext.Room.FirstOrDefault(r => r.Id == roomId);
        }


        public async Task<List<Room>> GetAvailableRooms(DateTime checkInDate, DateTime checkOutDate)
        {
            var availableRooms = await _dbContext.Room
       .Where(room => !_dbContext.RoomReservationcs
           .Any(reservation =>
               reservation.RoomId == room.Id &&
               reservation.CheckInDate.Date <= checkOutDate.Date &&
               reservation.CheckOutDate.Date >= checkInDate.Date &&
               reservation.Status == 1)) // Add status condition here
       .ToListAsync();


            return availableRooms;
        }


        public async Task<List<Room>> GetAvailableRoomsEdit(DateTime checkInDate, DateTime checkOutDate, int resId)
        {
            var availableRooms = await _dbContext.Room
       .Where(room => !_dbContext.RoomReservationcs
           .Any(reservation =>
               reservation.RoomId == room.Id &&
               reservation.CheckInDate.Date <= checkOutDate.Date &&
               reservation.CheckOutDate.Date >= checkInDate.Date &&
               reservation.Status == 1 &&
               reservation.ResevationId != resId)) // Add status condition here
       .ToListAsync();


            return availableRooms;
        }
    }
}
