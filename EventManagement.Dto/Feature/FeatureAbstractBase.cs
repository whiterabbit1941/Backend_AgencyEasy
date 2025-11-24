using System;

namespace EventManagement.Dto
{
    public abstract class FeatureAbstractBase
    {
        /// <summary>
        /// Feature Id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Feature Name.
        /// </summary>
        public string Descriptions { get; set; }

    }
}
