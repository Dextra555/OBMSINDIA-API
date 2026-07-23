using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OBMS.WebAPI.Models.Domain
{

    [Table("BranchPaymentForBranch")]
    [Keyless]
    public class BranchPaymentForBranch
    {

        public int? ID { get; set; }
        public decimal? PaymentID { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? BName { get; set; }
        public decimal? Amount { get; set; }
    }
}
