using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RazorPagesMovie.Models;

namespace RazorPagesMovie.Data
{
    public class RazorPagesMovieContext : IdentityDbContext<ApplicationUser>
    {
        public RazorPagesMovieContext(DbContextOptions<RazorPagesMovieContext> options)
            : base(options)
        {
        }

        public DbSet<RazorPagesMovie.Models.Room> Room { get; set; } = default!;
        public DbSet<RazorPagesMovie.Models.Reservations> Reservations { get; set; } = default!;

        public DbSet<RazorPagesMovie.Models.Food> Food { get; set; } = default!;

        public DbSet<RazorPagesMovie.Models.Beverage> Beverage { get; set; } = default!;
        public DbSet<RazorPagesMovie.Models.ThirdPartyHandlers> ThirdPartyHandlers { get; set; } = default!;

        public DbSet<Bill> BillingTransactions { get; set; } = default!;

        public DbSet<RazorPagesMovie.Models.RoomReservationcs> RoomReservationcs { get; set; } = default!;

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }

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
                       SELECT 
                       SUM(
                         CASE 
                           WHEN res.SelectedCurrency = 'USD' THEN room.PriceUSD * DATEDIFF(DAY, a.CheckInDate, a.CheckOutDate)
                           ELSE room.Price * DATEDIFF(DAY, a.CheckInDate, a.CheckOutDate)
                         END
                       ) AS TotalRoomCharge
                     FROM 
                       Room room, Reservations res, RoomReservationcs a
                     WHERE 
                       res.Id = a.ResevationId
                       AND a.RoomId = room.Id
                       AND res.Id = @reservationId
                       AND a.Status = 1";  // Add status condition here


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




        public async Task<decimal> CalculateDiscountedPriceAsync(int reservationId)
        {
            // Write the raw SQL query to calculate the total room charges
            var query = @"
                        SELECT 
                         SUM(
                           CASE 
                             WHEN res.SelectedCurrency = 'USD' THEN (room.PriceUSD * DATEDIFF(DAY, a.CheckInDate, a.CheckOutDate)) * res.DiscountRate / 100
                             ELSE (room.Price * DATEDIFF(DAY, a.CheckInDate, a.CheckOutDate)) * res.DiscountRate / 100
                           END
                         ) * -1 AS DiscountAmount
                       FROM 
                         Room room, Reservations res, RoomReservationcs a
                       WHERE 
                         res.Id = a.ResevationId
                         AND a.RoomId = room.Id
                         AND res.Id = @reservationId
                         AND a.Status = 1 ;";  // Add status condition here


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

        public async Task<decimal> CalculateDiscountedTotalAsync(int reservationId)
        {
            // Write the raw SQL query to calculate the total room charges
            var query = @"
                        SELECT sum(bill.itemPrice * bill.itemQty)
                        FROM BillingTransactions bill, Reservations res
                        where bill.InvoiceNo = res.Id 
                        and bill.InvoiceNo = @reservationId
                        and bill.tranStatus = 1
                        ;";  // Add status condition here


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

        public async Task<decimal> CalculateTotalWithoutDiscountAsync(int reservationId)
        {
            // Write the raw SQL query to calculate the total room charges
            var query = @"
                        SELECT sum(bill.itemPrice * bill.itemQty)  + max(discount.DiscountAmount)
                        FROM BillingTransactions bill, Reservations res,
                         (SELECT 
                         SUM(
                           CASE 
                             WHEN res.SelectedCurrency = 'USD' THEN (room.PriceUSD * DATEDIFF(DAY, a.CheckInDate, a.CheckOutDate)) * res.DiscountRate / 100
                             ELSE (room.Price * DATEDIFF(DAY, a.CheckInDate, a.CheckOutDate)) * res.DiscountRate / 100
                           END
                         )   AS DiscountAmount
                       FROM 
                         Room room, Reservations res, RoomReservationcs a
                       WHERE 
                         res.Id = a.ResevationId
                         AND a.RoomId = room.Id
                         AND res.Id = @reservationId
                         AND a.Status = 1 
                         ) discount
                        where bill.InvoiceNo = res.Id 
                        and bill.InvoiceNo = @reservationId
                        and bill.tranStatus = 1;";  // Add status condition here


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





        public async Task<DashboardViewModel> GetDashboardValuesAsync()
        {
            var query = @"
WITH TotalReservations AS (
    SELECT COUNT(*) AS total_reservation
    FROM Reservations
),
UpcomingReservations AS (
    SELECT COUNT(*) AS upcoming_reservation
    FROM Reservations
    WHERE CheckInDate > CAST(GETDATE() AS DATE)
),
ActiveReservations AS (
    SELECT COUNT(*) AS active_reservation
    FROM Reservations
    WHERE status  = 0
),

TotalRooms AS (
    SELECT COUNT(*) AS totRooms
    FROM Room
    WHERE IsAvailable  = 1
),

AvailableRoomCountToday as (SELECT count(r.id) as available_today
FROM Room r
WHERE NOT EXISTS (
    SELECT 1
    FROM RoomReservationcs rr
    WHERE rr.RoomId = r.Id
      AND rr.Status = 1
      AND rr.CheckInDate <= GETDATE()
      AND rr.CheckOutDate >= GETDATE()
)
),
IntendedCheckinsToday as (
SELECT COUNT(DISTINCT res.Id) AS IntendedCheckInCount
FROM RoomReservationcs rr
JOIN Reservations res ON rr.ResevationId = res.Id
WHERE rr.CheckInDate = CAST(GETDATE() AS DATE) 
  AND res.Status = 0),
IntendedCheckoutsToday as (
SELECT COUNT(DISTINCT res.Id) AS IntendedCheckOutCount
FROM RoomReservationcs rr
JOIN Reservations res ON rr.ResevationId = res.Id
WHERE rr.CheckOutDate = CAST(GETDATE() AS DATE) 
AND res.Status =1

),
IntendedCheckinTomorrow as (
SELECT COUNT(DISTINCT res.Id) AS IntendedCheckInCountTomorrow
FROM RoomReservationcs rr
JOIN Reservations res ON rr.ResevationId = res.Id
WHERE rr.CheckInDate = CAST(DATEADD(DAY, 1, GETDATE()) AS DATE)
AND res.Status = 0
),

IntendedCheckOutTomorrow as (
SELECT COUNT(DISTINCT res.Id) AS IntendedCheckOutCountTomorrow
FROM RoomReservationcs rr
JOIN Reservations res ON rr.ResevationId = res.Id
WHERE rr.CheckOutDate = CAST(DATEADD(DAY, 1, GETDATE()) AS DATE)
AND res.Status = 1
)
SELECT 
    t.total_reservation,
    u.upcoming_reservation,
	a.active_reservation,
	r.totRooms,
	av.available_today,
	x.IntendedCheckInCount,
	y.IntendedCheckOutCount,
	z.IntendedCheckInCountTomorrow,
	b.IntendedCheckOutCountTomorrow
FROM 
    TotalReservations t,
    UpcomingReservations u,
	ActiveReservations a,
	TotalRooms r,
	AvailableRoomCountToday av,
	IntendedCheckinsToday x,
	IntendedCheckoutsToday y,
	IntendedCheckinTomorrow z,
	IntendedCheckOutTomorrow b;";

            using (var command = this.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = query;

                await this.Database.OpenConnectionAsync();

                using (var result = await command.ExecuteReaderAsync())
                {
                    if (await result.ReadAsync())
                    {
                        var model = new DashboardViewModel
                        {
                            TotalReservation = result.IsDBNull(0) ? 0 : result.GetInt32(0),
                            UpcomingReservation = result.IsDBNull(1) ? 0 : result.GetInt32(1),
                            ActiveReservation = result.IsDBNull(2) ? 0 : result.GetInt32(2),
                            TotalRooms = result.IsDBNull(3) ? 0 : result.GetInt32(3),
                            AvailableRoomCountToday = result.IsDBNull(4) ? 0 : result.GetInt32(4),
                            IntendedCheckinCountToday = result.IsDBNull(5) ? 0 : result.GetInt32(5),
                            IntendedCheckOutCountToday = result.IsDBNull(6) ? 0 : result.GetInt32(6),
                            IntendedCheckinCountTomorrow = result.IsDBNull(7) ? 0 : result.GetInt32(7),
                            IntendedCheckOutCountTomorrow = result.IsDBNull(8) ? 0 : result.GetInt32(8)

                        };
                        return model;
                    }
                }
            }

            return new DashboardViewModel
            {
                TotalReservation = 0,
                UpcomingReservation = 0,
                ActiveReservation = 0
            };
        }




    }
    public class RoomChargeResult
    {
        public decimal TotalRoomCharge { get; set; }
    }
}