namespace RazorPagesMovie.Models
{
    public class SysPara
    {
        public int Id { get; set; }

        public string Key { get; set; }

        public string Value { get; set; }

        public DateTime createdDate { get; set; }= DateTime.Now;



    }
}
