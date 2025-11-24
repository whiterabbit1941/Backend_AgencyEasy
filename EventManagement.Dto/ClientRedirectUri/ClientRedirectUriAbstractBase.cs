using System;

namespace EventManagement.Dto
{
    public abstract class ClientRedirectUriAbstractBase
    {
        /// <summary>
        /// ClientRedirectUri Id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ClientRedirectUri Name.
        /// </summary>
        public string Name { get; set; }

    }
}
