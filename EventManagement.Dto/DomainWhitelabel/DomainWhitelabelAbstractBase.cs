using EventManagement.Domain.Entities;
using System;

namespace EventManagement.Dto
{
    public abstract class DomainWhitelabelAbstractBase
    {
        /// <summary>
        /// DomainWhitelabel Id.
        /// </summary>
        public Guid Id { get; set; }
        public virtual Company Company { get; set; }
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
