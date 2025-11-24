using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Marvin.IDP.Entities
{
    public class Edge_ClientUser : AuditableEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public virtual Edge_Client Client { get; set; }

        [Required]
        [ForeignKey("Edge_ClientAccount")]
        public Guid ClientId { get; set; }

        [Required]
        [ForeignKey("AspNetUsers")]
        public string UserID { get; set; }

        public virtual User User { get; set; }

        public virtual Edge_Account Account { get; set; }

        [Required]
        [ForeignKey("Edge_Account")]
        public Guid AccountId { get; set; }

        [Required]
        [MaxLength(250)]
        public string Role { get; set; }

    }
}
