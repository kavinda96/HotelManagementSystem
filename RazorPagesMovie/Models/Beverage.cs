using System.ComponentModel.DataAnnotations;

namespace RazorPagesMovie.Models;

using Microsoft.EntityFrameworkCore.Migrations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
public class Beverage
{
    public int Id { get; set; }

    [Display(Name = "Beverage")]
    public string? BeverageName { get; set; }

    [Display(Name = "Beverage Description")]
    public string? BeverageDescription { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    [Required]
    [RegularExpression(@"^\d+(\.\d+)?$", ErrorMessage = "Please enter a valid number.")]
    public decimal Price { get; set; }

    public int? IsAvailable { get; set; }
}

