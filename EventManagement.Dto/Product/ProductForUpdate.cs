using System;
using System.ComponentModel.DataAnnotations;

namespace EventManagement.Dto
{
    /// <summary>
    /// Product Update Model.
    /// </summary>
    public class ProductForUpdate : ProductAbstractBase
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
