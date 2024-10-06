using System.ComponentModel.DataAnnotations;

namespace RazorPagesMovie.Models
{
    public class ThirdPartyHandlers
    {
        public int Id { get; set; }

        [Display(Name = "Name")]
        [Required]
        [StringLength(100)]
        public string CompanyName { get; set; }


        [StringLength(200)]
        public string Address { get; set; }
     

        [Display(Name = "Contact No.")]
        [Required]
        [Phone]
        public string ContactNo { get; set; }


        [Display(Name = "Email")]
        [Required]
        [EmailAddress]
        public string Email { get; set; } 


    }
}
