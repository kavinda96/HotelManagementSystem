using RazorPagesMovie.Data;
using RazorPagesMovie.Models;

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
    }
}
