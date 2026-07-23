using System;
 
namespace OBMS.WebAPI.Models.DTO
{
    public class ClientStatementDto
    {
        public DateTime TransactionDate { get; set; }
        public string TransactionType { get; set; }
        public string ReferenceNo { get; set; }
        public string Description { get; set; }
        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }
        public decimal Balance { get; set; }
        public string Branch { get; set; }
        public string Client { get; set; }
    }
}
 