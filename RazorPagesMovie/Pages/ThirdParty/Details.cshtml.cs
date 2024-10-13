using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorPagesMovie.Data;
using RazorPagesMovie.Models;

namespace RazorPagesMovie.Pages.ThirdParty
{
    [Authorize]
    public class DetailsModel : PageModel
    {
        private readonly RazorPagesMovie.Data.RazorPagesMovieContext _context;

        public DetailsModel(RazorPagesMovie.Data.RazorPagesMovieContext context)
        {
            _context = context;
        }

        public ThirdPartyHandlers ThirdPartyHandlers { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var thirdpartyhandlers = await _context.ThirdPartyHandlers.FirstOrDefaultAsync(m => m.Id == id);
            if (thirdpartyhandlers == null)
            {
                return NotFound();
            }
            else
            {
                ThirdPartyHandlers = thirdpartyhandlers;
            }
            return Page();
        }
    }
}
