using System.ComponentModel.DataAnnotations;

namespace RazorPagesMovie.Models;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
    public class Food
    {
        public int Id { get; set; }

        [Display(Name = "Food")]
        public string? FoodName { get; set; }

        [Display(Name = "Food Description")]
        public string? FoodDescription { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        [Required]
        [RegularExpression(@"^\d+(\.\d+)?$", ErrorMessage = "Please enter a valid number.")]
        public decimal Price { get; set; }

        public int? IsAvailable { get; set; }
    }

