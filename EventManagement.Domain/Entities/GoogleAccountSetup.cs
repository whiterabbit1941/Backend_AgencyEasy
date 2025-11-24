using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventManagement.Domain.Entities
{
    public class GoogleAccountSetup : AuditableEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

                
        public string AccessToken { get; set; }

               
        public string RefreshToken { get; set; }

        
        [MaxLength(250)]
        public string UserId { get; set; }

        
        [MaxLength(250)]
        public string UserName { get; set; }
        
                
        public bool IsAuthorize { get; set; }

        public Guid CompanyId { get; set; }
        public string AccountType { get; set; }

    }
}
