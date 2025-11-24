using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EventManagement.Dto
{
    /// <summary>
    /// CampaignLinkedin Model
    /// </summary>
    public class CampaignLinkedinDto : CampaignLinkedinAbstractBase
    {
        
    }

    public class LinkedinToken
    {
        public string access_token { get; set; }

        public string expires_in { get; set; }

        public string refresh_token { get; set; }

        public string refresh_token_expires_in { get; set; }

    }

    public class LinkedinElement
    {
        [JsonProperty("organization~")]
        public LinkedinOrganization organization1 { get; set; }
        public string role { get; set; }
        [JsonProperty("organization")]
        public string organization { get; set; }
        [JsonProperty("roleAssignee")]
        public string roleAssignee { get; set; }
        public string state { get; set; }
        [JsonProperty("roleAssignee~")]
        public LinkedinRoleAssignee roleAssignee1 { get; set; }
    }

    public class LinkedinOrganization
    {
        public string localizedName { get; set; }
    }

    public class LinkedinRoleAssignee
    {
        public string localizedLastName { get; set; }
        public string localizedFirstName { get; set; }
    }

    public class LinkedinRoot
    {
        public List<LinkedinElement> elements { get; set; }
    }

}
 