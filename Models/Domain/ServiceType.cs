using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OBMS.WebAPI.Models.Domain
{
    [Table("ServiceType")]
    public class ServiceType
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string ServiceName { get; set; }

        [StringLength(20)]
        public string ServiceCode { get; set; }

        [StringLength(200)]
        public string Description { get; set; }


        [Required]
        [StringLength(8)]
        public string HSNCode { get; set; }

        [StringLength(50)]
        public string? PricingModel { get; set; } = "Standard";

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Required]
        [StringLength(50)]
        public string CreatedBy { get; set; }

        public DateTime? LastUpdatedDate { get; set; }

        [StringLength(50)]
        public string LastUpdatedBy { get; set; }

        public virtual ICollection<AgreementDetails> AgreementDetails { get; set; }
    }

}
