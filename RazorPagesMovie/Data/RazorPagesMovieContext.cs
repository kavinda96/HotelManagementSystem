using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    }


}
