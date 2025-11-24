using System;

namespace EventManagement.Dto
{
    public abstract class ProductAbstractBase
    {
        /// <summary>
        /// Product Id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Product Name.
        /// </summary>
        public string Name { get; set; }

    }
}
