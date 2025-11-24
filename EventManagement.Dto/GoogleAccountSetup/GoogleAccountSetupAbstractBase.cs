using System;

namespace EventManagement.Dto
{
    public abstract class GoogleAccountSetupAbstractBase
    {
        /// <summary>
        /// GoogleAccountSetup Id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// GoogleAccountSetup Name.
        /// </summary>
        public string Name { get; set; }

    }
}
