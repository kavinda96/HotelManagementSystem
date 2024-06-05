using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorPagesMovie.Models;

namespace RazorPagesMovie.Pages.Homepage
{
    public class IndexModel : PageModel
    {
        private readonly RazorPagesMovie.Data.RazorPagesMovieContext _context;
        public IndexModel(RazorPagesMovie.Data.RazorPagesMovieContext context)
        {
            _context = context;
        }

        public int TotalRooms { get; set; }
        public int OccupiedRooms { get; set; }
        public int TotalReservations { get; set; }
        public int ActivelReservations { get; set; }

        public DashboardViewModel DashboardData { get; set; }
        public async Task OnGetAsync()
        {

            DashboardData = await _context.GetDashboardValuesAsync();

        }
    }
}
