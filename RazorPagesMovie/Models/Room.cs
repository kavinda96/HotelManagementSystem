using System.ComponentModel.DataAnnotations;

namespace RazorPagesMovie.Models;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

public class Room
{
    public int Id { get; set; }

    [Display(Name = "Room No")]
    public string? RoomNo { get; set; }

    [Display(Name = "Room Type")]
    public string? RoomType { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    [Required]
    [RegularExpression(@"^\d+(\.\d+)?$", ErrorMessage = "Please enter a valid number.")]
    public decimal Price { get; set; }

    [Display(Name = "Beds Count")]
    [Required]
    [RegularExpression(@"^\d+$", ErrorMessage = "Please enter numbers only.")]
    public int? BedsCount { get; set; }

    public int? IsAvailable { get; set; }

    [NotMapped]
    public bool IsSelected { get; set; }
}

