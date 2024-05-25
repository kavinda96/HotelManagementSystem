using RazorPagesMovie.Data;
using RazorPagesMovie.Models;

namespace RazorPagesMovie.Services
{
    public class BillingTransactionService
    {
        private readonly RazorPagesMovieContext _dbContext;


        public BillingTransactionService(RazorPagesMovieContext dbContext)
        {
            _dbContext = dbContext;
        }

        private static readonly List<Bill> BillingTransactions = new List<Bill>();

        public List<Bill> GetBillingTransactionsByReservationId(long? reservationId)
        {
            return _dbContext.BillingTransactions.Where(t => t.InvoiceNo == reservationId && t.tranStatus !=2 ).ToList();


        }
        public List<Bill> GetBillingTransactionsyBillingId(long? billId)
        {
            return _dbContext.BillingTransactions.Where(t => t.Id == billId).ToList();


        }

        public void AddBillingTransaction(Bill transaction)
        {
            transaction.Id = BillingTransactions.Count + 1; // Simulate auto-increment ID
            BillingTransactions.Add(transaction);
        }
    }
}
