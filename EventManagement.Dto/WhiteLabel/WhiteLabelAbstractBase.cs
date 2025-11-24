using System;

namespace EventManagement.Dto
{
    public abstract class WhiteLabelAbstractBase
    {
        /// <summary>
        /// WhiteLabel Id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// WhiteLabel Name.
        /// </summary>
        public string Name { get; set; }

    }
}
