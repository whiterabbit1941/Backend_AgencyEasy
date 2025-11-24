using System;
using System.ComponentModel.DataAnnotations;

namespace EventManagement.Dto
{
    /// <summary>
    /// Company Update Model.
    /// </summary>
    public class CompanyForUpdate : CompanyAbstractBase
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

        public bool IsApproved { get; set; }

        public string CompanyImageUrl { get; set; }

        public string Fevicon { get; set; }

        public string VatNo { get; set; }
        public string Theme { get; set; }

        public bool IsAllowMarketPlace { get; set; }


    }
}
