using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventManagement.Domain.Entities
{
    public class StripeCoupon : AuditableEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(250)]
        [ForeignKey("Company")]
        public Guid CompanyId { get; set; }

        public string CouponCode { get; set; }

        public DateTime ExpiredAt { get; set; }

        public string DiscountPercent { get; set; }

        public string GeneratedBy { get; set; }

    }
}
