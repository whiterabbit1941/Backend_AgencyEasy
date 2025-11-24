using System;
using System.ComponentModel.DataAnnotations;

namespace EventManagement.Dto
{
    public class GoogleAccountSetupForCreation : GoogleAccountSetupAbstractBase
    {
        public Guid Id { get; set; }

        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }


        public string UserId { get; set; }

        public string UserName { get; set; }

        public bool IsAuthorize { get; set; }
        public Guid CompanyId { get; set; }
        public string AccountType { get; set; }
    }
}
