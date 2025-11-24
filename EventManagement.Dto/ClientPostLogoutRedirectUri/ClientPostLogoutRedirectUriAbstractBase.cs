using System;

namespace EventManagement.Dto
{
    public abstract class ClientPostLogoutRedirectUriAbstractBase
    {
        /// <summary>
        /// ClientPostLogoutRedirectUri Id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// ClientPostLogoutRedirectUri Name.
        /// </summary>
        public string Name { get; set; }

    }
}
