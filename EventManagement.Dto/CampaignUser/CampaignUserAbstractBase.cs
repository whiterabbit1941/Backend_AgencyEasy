using System;

namespace EventManagement.Dto
{
    public abstract class CampaignUserAbstractBase
    {
        /// <summary>
        /// CampaignUser Id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// CampaignUser Name.
        /// </summary>
        public string Name { get; set; }

    }
}
