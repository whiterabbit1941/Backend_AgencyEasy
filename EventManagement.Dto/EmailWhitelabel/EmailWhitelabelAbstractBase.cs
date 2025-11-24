using System;

namespace EventManagement.Dto
{
    public abstract class EmailWhitelabelAbstractBase
    {
        /// <summary>
        /// EmailWhitelabel Id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// EmailWhitelabel Name.
        /// </summary>
        public string Name { get; set; }

    }
}
