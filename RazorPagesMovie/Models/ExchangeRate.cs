using System.ComponentModel.DataAnnotations.Schema;

namespace RazorPagesMovie.Models
{
    public class ExchangeRate
    {
        public int Id { get; set; }

        public DateTime CreatedDate { get; set; }

        public string ExchangeRateName { get; set;}

        [Column(TypeName = "decimal(18, 6)")] // Ensure this is present
        public decimal ExchangeRateAmount { get; set; }
        
    }
}
