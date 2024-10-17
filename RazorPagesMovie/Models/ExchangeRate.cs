namespace RazorPagesMovie.Models
{
    public class ExchangeRate
    {
        public int Id { get; set; }

        public DateTime CreatedDate { get; set; }

        public string ExchangeRateName { get; set;}

        public decimal ExchangeRateAmount {  get; set;}
    }
}
