using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventManagement.Domain.Entities
{
    public class EmailWhitelabel : AuditableEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public Guid CompanyID { get; set; }
        public int DomainID { get; set; }
        public string DomainName { get; set; }
        public string CnameHost { get; set; }
        public string CnameType { get; set; }
        public string CnamePointsTo { get; set; }
        public string DomainKey1Type { get; set; }
        public string DomainKey1PointsTo { get; set; }
        public string DomainKey2Type { get; set; }
        public string DomainKey2PointsTo { get; set; }
        public bool IsVerify { get; set; }

        [ForeignKey("CompanyID")]
        public virtual Company Company { get; set; }
    }
}
