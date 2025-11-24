using System;

namespace EventManagement.Dto
{
    /// <summary>
    /// GoogleAccountSetup Model
    /// </summary>
    public class GoogleAccountSetupDto : GoogleAccountSetupAbstractBase
    {

        public Guid Id { get; set; }


        public string AccessToken { get; set; }


        public string RefreshToken { get; set; }



        public string UserId { get; set; }



        public string UserName { get; set; }


        public bool IsAuthorize { get; set; }
    }

}
 