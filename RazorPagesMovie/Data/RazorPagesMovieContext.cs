using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RazorPagesMovie.Models;

namespace RazorPagesMovie.Data
{
    public class RazorPagesMovieContext : DbContext
    {
        public RazorPagesMovieContext (DbContextOptions<RazorPagesMovieContext> options)
            : base(options)
        {
        }

        public DbSet<RazorPagesMovie.Models.Room> Room { get; set; } = default!;
        public DbSet<RazorPagesMovie.Models.Reservations> Reservations { get; set; } = default!;

        public DbSet<Bill> BillingTransactions { get; set; } = default!;

        public DbSet<RazorPagesMovie.Models.RoomReservationcs> RoomReservationcs { get; set; } = default!;

        public DbSet<RoomChargeResult> RoomChargeResults { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure RoomChargeResult as keyless
            modelBuilder.Entity<RoomChargeResult>().HasNoKey();
        }


        public async Task<decimal> CalculateRoomChargeAsync(int reservationId)
        {
            // Write the raw SQL query to calculate the total room charges
            var query = @"
            select SUM(room.Price * DATEDIFF(DAY, a.CheckInDate, a.CheckOutDate) ) as TotalRoomCharge from Room room, Reservations res, RoomReservationcs a
            where res.Id = a.ResevationId
            and a.RoomId = room.Id
            and res.Id = @reservationId";

            // Execute the raw SQL query and get the result

            var parameter = new SqlParameter("@reservationId", reservationId);

            using (var command = this.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = query;
                command.Parameters.Add(parameter);

                await this.Database.OpenConnectionAsync();

                using (var result = await command.ExecuteReaderAsync())
                {
                    if (await result.ReadAsync())
                    {
                        return result.IsDBNull(0) ? 0 : result.GetDecimal(0);
                    }
                }
            }

            return 0;
        }

    }


}
public class RoomChargeResult
{
    public decimal TotalRoomCharge { get; set; }
}