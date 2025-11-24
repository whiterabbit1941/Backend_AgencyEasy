using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EventManagement.Dto
{
    public abstract class CampaignMailchimpAbstractBase
    {
        /// <summary>
        /// CampaignMailchimp Id.
        /// </summary>
        public Guid Id { get; set; }
        public string AccountId { get; set; }
        public string AccountName { get; set; }

        //Mailchimp Marketing access tokens do not expire, so you don't need to use a refresh_token .
        //The access token will remain valid unless the user revokes your application's permission to access their account.
        public string AccessToken { get; set; }

        public string ApiEndpoint { get; set; }
        public Guid CampaignID { get; set; }
    }


}
