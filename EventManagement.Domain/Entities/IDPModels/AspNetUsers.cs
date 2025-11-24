using EventManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanaceManagement.API.Models
{
    public partial class AspNetUsers : AuditableEntity
    {
        public AspNetUsers()
        {
            AspNetUserClaims = new HashSet<AspNetUserClaims>();
            AspNetUserLogins = new HashSet<AspNetUserLogins>();
            AspNetUserRoles = new HashSet<AspNetUserRoles>();
            AspNetUserTokens = new HashSet<AspNetUserTokens>();
        }

        public string Id { get; set; }
        public int AccessFailedCount { get; set; }
        public string ConcurrencyStamp { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool LockoutEnabled { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public string NormalizedEmail { get; set; }
        public string NormalizedUserName { get; set; }
        public string PasswordHash { get; set; }
        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public string SecurityStamp { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public bool ShowDemoProject { get; set; }
        public string UserName { get; set; }
        public string Database { get; set; }
        public bool IsActive { get; set; }
        public DateTime Birthday { get; set; }
        public string Birthplace { get; set; }
        public string Gender { get; set; }
        public string LivesIn { get; set; }
        public string Occupation { get; set; }
        [MaxLength(200)]
        public string FName { get; set; }
        [MaxLength(200)]
        public string LName { get; set; }
        public string ImageUrl { get; set; }

        public bool Role { get; set; }

        public Guid CompanyID { get; set; }

        public string Address { get; set; }

        public Guid RowGuid { get; set; }


        public virtual ICollection<AspNetUserClaims> AspNetUserClaims { get; set; }
        public virtual ICollection<AspNetUserLogins> AspNetUserLogins { get; set; }
        public virtual ICollection<AspNetUserRoles> AspNetUserRoles { get; set; }
        public virtual ICollection<AspNetUserTokens> AspNetUserTokens { get; set; }
   
        [ForeignKey("CompanyID")]
        public virtual Company Company { get; set; }
    }
}
