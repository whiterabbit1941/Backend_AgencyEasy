using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventManagement.Domain.Entities
{
    public class DomainWhitelabel : AuditableEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public virtual Company Company { get; set; }
        [ForeignKey("CompanyID")]
        public Guid CompanyID { get; set; }
        public string DomainName { get; set; }
        public string AlternateDomainName { get; set; }
        public string Origin { get; set; }
        public string CnameType { get; set; }
        public string CnameHost { get; set; }
        public string CnamePointsTo { get; set; }
        public string CertificateARN { get; set; }
        public string DistributionId { get; set; }
        public string Status { get; set; }
        public bool Certificate { get; set; }
        public string AmplifyAppId { get; set; }
    }
}
