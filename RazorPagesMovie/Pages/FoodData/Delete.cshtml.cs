using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorPagesMovie.Data;
using RazorPagesMovie.Models;

namespace RazorPagesMovie.Pages.FoodData
{
    public class DeleteModel : PageModel
    {
        private readonly RazorPagesMovie.Data.RazorPagesMovieContext _context;

        public DeleteModel(RazorPagesMovie.Data.RazorPagesMovieContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Food Food { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var food = await _context.Food.FirstOrDefaultAsync(m => m.Id == id);

            if (food == null)
            {
                return NotFound();
            }
            else
            {
                Food = food;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var food = await _context.Food.FindAsync(id);
            if (food != null)
            {
                Food = food;
                _context.Food.Remove(Food);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
