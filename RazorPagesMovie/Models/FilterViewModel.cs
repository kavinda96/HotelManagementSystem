using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace RazorPagesMovie.Models
{
    public class FilterViewModel
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string FilterColumn { get; set; }
        public string FilterValue { get; set; }
        public List<SelectListItem> FilterColumns { get; set; }
    }
}
