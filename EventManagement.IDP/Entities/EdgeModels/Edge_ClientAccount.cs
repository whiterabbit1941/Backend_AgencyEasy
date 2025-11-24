using Marvin.IDP.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Marvin.IDP.Entities
{
    public class Edge_Client : AuditableEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public virtual Edge_Account Account { get; set; }

        [Required]
        [ForeignKey("Edge_Account")]
        public Guid AccountId { get; set; }

        [Required]
        [MaxLength(100)]
        public string DBName { get; set; }

        [Required]
        [MaxLength(300)]
        public string OrganisationName { get; set; }

        [Required]
        [MaxLength(300)]
        public string BillingManagedBy { get; set; }

        public string BillingDetail { get; set; }

        public string PowerBIMasterUserID { get; set; }

        public string PowerBIMasterUserPassword { get; set; }

        public DateTime TrialPeriod { get; set; }

        public string ImageUrl { get; set; }

        public int? ClientType { get; set; }
        public int? BusinessStructure { get; set; }
        public int? Industry { get; set; }
        public int? Sector { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string Zipcode { get; set; }
        public int? Region { get; set; }
        public string Telephone { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
        public string PrimaryContact { get; set; }

    }
}
