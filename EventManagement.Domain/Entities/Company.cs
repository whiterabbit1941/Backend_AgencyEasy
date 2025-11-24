using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventManagement.Domain.Entities
{
    public class Company : AuditableEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
         
        public string Name { get; set; }

        
        public string Website { get; set; }

       
        public string Phone { get; set; }

        
        public string Timezone { get; set; }

         
        public string Address { get; set; }

         
        public string ZipCode { get; set; }

         
        public string City { get; set; }

         
        public string State { get; set; }

         
        public string Country { get; set; }
         
        public string Description { get; set; }
         
        public string Branding { get; set; }
        public string Email { get; set; }
        public string CompanyType { get; set; }
        public string CompanyImageUrl { get; set; }

        public bool IsApproved { get; set; }

        public bool IsAllowMarketPlace { get; set; }

        public Guid RowGuid { get; set; }

        public string VatNo { get; set; }

        public string SubDomain { get; set; }

        public string Fevicon { get; set; }

        public string DashboardLogo { get; set; }
        public string Theme { get; set; }

    }
}
