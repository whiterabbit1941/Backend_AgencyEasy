using System;

namespace EventManagement.Dto
{
    public abstract class EventAbstractBase
    {
        /// <summary>
        /// Event Id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Event Name.
        /// </summary>
        public string Name { get; set; }

    }
}
