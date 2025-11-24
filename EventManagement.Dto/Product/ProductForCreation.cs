using System;
using System.ComponentModel.DataAnnotations;

namespace EventManagement.Dto
{
    public class ProductForCreation : ProductAbstractBase
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Guid CompanyID { get; set; }
    }
}
