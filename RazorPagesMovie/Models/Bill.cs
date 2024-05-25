using System.ComponentModel.DataAnnotations;

namespace RazorPagesMovie.Models
{
    public class Bill
    {
        public int Id { get; set; }

        [Display(Name = "Invoice No")]
      //  [Required]
        public long? InvoiceNo { get; set; }

        [Display(Name = "Billing Category")]
        public string? Category { get; set; }

        [Display(Name = "Billing Item")]
      // [Required]
        public string? BillingItem { get; set; }

        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Item Price")]
      //  [Required]
        public decimal? ItemPrice { get; set; }

        [Display(Name = "Item Qty")]
      //  [Required]
        public int? ItemQty { get; set; }

        public decimal? itemVat { get; set; }

        public int ? categoryId { get; set; }

        public DateTime? createdDate { get; set; } = DateTime.Now;

        public int? tranStatus { get; set; } = 1;

    }
}
