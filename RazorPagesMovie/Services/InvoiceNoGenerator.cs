using Microsoft.Data.SqlClient;
using RazorPagesMovie.Data;
using System;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
namespace RazorPagesMovie.Services
{
    public  class InvoiceNoGenerator
    {

     
        private readonly RazorPagesMovieContext _dbContext;
        private readonly IConfiguration _configuration;


        public InvoiceNoGenerator(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GetConnectionString()
        {
            return _configuration.GetConnectionString("RazorPagesMovieContext"); // Replace "DefaultConnection" with your actual connection string name
        }
        public long GenerateInvoiceNumber()
        {



            

            long invoiceNoNext = GetLatestInvoiceNumberNew()+ 1;

            return invoiceNoNext;





        }


        public string GetLatestInvoiceNumber()
        {
            string connectionString = GetConnectionString(); // Replace with your actual connection string
            string sql = "SELECT TOP 1 MasterbillId FROM Reservations ORDER BY Id DESC";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    string latestInvoiceNumber = (string)command.ExecuteScalar();
                    return latestInvoiceNumber;
                }
            }
        }

        public long GetLatestInvoiceNumberNew()
        {
            string connectionString = GetConnectionString(); // Replace with your actual connection string
            string sql = "SELECT TOP 1 MasterbillId FROM Reservations ORDER BY Id DESC";
            string sqlcount = "SELECT count(*)   FROM Reservations ";


            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();


                using (SqlCommand command1 = new SqlCommand(sqlcount, connection))
                {
                    int intValue1 = 0;
                    intValue1 = (int)command1.ExecuteScalar();

                    if (intValue1 > 0)
                    {
                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {


                            long intValue = 0;
                            intValue = (long)command.ExecuteScalar(); // Cast to integer if necessary

                            if (intValue == null)
                            {
                                intValue = 0;
                                return intValue;
                            }
                            return intValue;

                        }
                    }
                    else
                    {
                        return 0;
                    }
                }
            }
        }

    }
}
