using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OBMS.WebAPI.Models.Domain
{
    [Table("IndianBanks")]
    public class IndianBanks
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string BankCode { get; set; }

        [Required]
        [StringLength(100)]
        public string BankName { get; set; }

        [Required]
        [StringLength(20)]
        public string IFSCCode { get; set; }

        [StringLength(20)]
        public string? MICRCode { get; set; }

        [StringLength(100)]
        public string? BranchName { get; set; }

        [StringLength(200)]
        public string? Address { get; set; }

        [StringLength(50)]
        public string? City { get; set; }

        [StringLength(50)]
        public string? State { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Required]
        [StringLength(50)]
        public string CreatedBy { get; set; }

        public DateTime? LastUpdatedDate { get; set; }

        [StringLength(50)]
        public string? LastUpdatedBy { get; set; }
    }
}
