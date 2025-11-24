using System;

namespace EventManagement.Dto
{
    /// <summary>
    /// Company Model
    /// </summary>
    public class CompanyDto 
    {
        public Guid Id { get; set; }

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

        public Guid CompanyID { get; set; }
        public string CompanyType { get; set; }
        public string Role { get; set; }
       public string CompanyImageUrl { get; set; }

       public bool IsApproved { get; set; }

        public bool IsAllowMarketPlace { get; set; }

        public string VatNo { get; set; }

        public Guid RowGuid { get; set; }

        public string SubDomain { get; set; }

        public string Fevicon { get; set; }

        public string DashboardLogo { get; set; }
        public string Theme { get; set; }

        public DateTime CreatedOn { get; set; }      
    }

    public class SuperAdminDashboard
    {
        public Guid Id { get; set; }
        public Guid CompanyID { get; set; }
        public string PlanName { get; set; }
        public bool? Active { get; set; }
        public string OldestUserEmail { get; set; }
        public DateTime? OldestUserCreatedOn { get; set; }
        public DateTime? ExpiredOn { get; set; }
        public string Role { get; set; }
        public string Name { get; set; }

        public string Phone { get; set; }

        public string Address { get; set; }

        public string ZipCode { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string Country { get; set; }

        public string CompanyType { get; set; }

        public string CompanyImageUrl { get; set; }

        public bool IsAllowMarketPlace { get; set; }

        public string Theme { get; set; }

        public string Fevicon { get; set; }
        public DateTime CreatedOn { get; set; }

    }

    public class CustomDomainCompanyInfo
    {
        public string Name { get; set; }

        public string CompanyImageUrl { get; set; }

        public string LinkedAgencyCompanyId { get; set; }

    }


    public class AwsImageUrl
    {
      

        public string CompanyImageUrl { get; set; }

        public string Fevicon { get; set; }

    }

}
 