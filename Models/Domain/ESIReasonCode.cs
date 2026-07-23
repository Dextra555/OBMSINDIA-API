using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OBMS.WebAPI.Models.Domain
{
    [Table("ESIReasonCode")]
    public class ESIReasonCode
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(10)]
        [Column("Reason_Code")]
        public string ReasonCode { get; set; }

        [Required]
        [StringLength(100)]
        [Column("Reason_Description")]
        public string ReasonDescription { get; set; }

        [Required]
        [Column("Is_Active")]
        public bool IsActive { get; set; } = true;

        [Column("Created_Date")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
