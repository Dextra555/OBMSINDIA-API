using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OBMS.WebAPI.Models.Domain
{
    [Table("TDSMaster")]
    public class TDSMaster
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [StringLength(100)]
        public string TDSName { get; set; } = null!;

        [Required]
        [Column(TypeName = "decimal(5,2)")]
        public decimal TDSPercentage { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
