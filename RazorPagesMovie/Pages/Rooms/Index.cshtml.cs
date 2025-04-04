﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RazorPagesMovie.Data;
using RazorPagesMovie.Models;

namespace RazorPagesMovie.Pages.Rooms
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly RazorPagesMovie.Data.RazorPagesMovieContext _context;
        public List<SelectListItem> RoomTypeNames { get; set; }

        public IndexModel(RazorPagesMovie.Data.RazorPagesMovieContext context)
        {
            _context = context;
           
        }

        public IList<Room> Room { get;set; } = default!;
        public IList<Room> VacantRooms { get; set; } = default!;

        [BindProperty(SupportsGet = true)]
        public string? SearchString { get; set; }

        public SelectList? RoomTypes { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? RoomType { get; set; }

      

        public async Task OnGetAsync()
        {
            RoomTypeNames = new List<SelectListItem>
            {
            new SelectListItem { Value = "1", Text = "Standard Double"},
            new SelectListItem { Value = "2", Text = "Standard Triple" },
            new SelectListItem { Value = "3", Text = "Deluxe A/C Double" },
            new SelectListItem { Value = "4", Text = "Deluxe A/C Triple" },
            new SelectListItem { Value = "5", Text = "Deluxe A/C Double Sea View Common Balcony" },
            new SelectListItem { Value = "6", Text = "Deluxe A/C Triple Sea View Common Balcony" },
            new SelectListItem { Value = "7", Text = "Deluxe A/C Double Sea View Private Balcony" },
            new SelectListItem { Value = "8", Text = "Deluxe A/C Triple Sea View Private Balcony" }

             };

            IQueryable<string> roomTypeQry = from m in _context.Room
                                            orderby m.RoomType
                                            select m.RoomType;



            var rooms = from m in _context.Room
                         select m;
            if (!string.IsNullOrEmpty(SearchString))
            {
                rooms = rooms.Where(s => s.RoomNo.Contains(SearchString));
            }

            if (!string.IsNullOrEmpty(RoomType))
            {
                rooms = rooms.Where(x => x.RoomType == this.RoomType);
            }



            VacantRooms = await _context.Room
            .Where(r => r.IsAvailable ==1)
            .ToListAsync();

            RoomTypes = new SelectList(await roomTypeQry.Distinct().ToListAsync());
            Room = await rooms.ToListAsync();
        }
    }
}
