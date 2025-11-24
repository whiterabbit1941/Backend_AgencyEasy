using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Marvin.IDP.Entities
{
    [Table("AspNetUsers")]
    public class User :IdentityUser
    {
     
        [Required]
        public bool IsActive { get; set; }

        [MaxLength(200)]
        public string Database { get; set; }

        public DateTime Birthday { get; set; }

        [MaxLength(200)]
        public string Birthplace { get; set; }

        [MaxLength(200)]
        public string Gender { get; set; }

        [MaxLength(200)]
        public string Occupation { get; set; }

        [MaxLength(200)]
        public string PhoneNumber { get; set; }

        [MaxLength(200)]
        public string LivesIn { get; set; }

        [NotMapped]
        public string CompanyID { get; set; }

        public string FName { get; set; }
        public string LName { get; set; }
        public bool ShowDemoProject { get; set; }
    }
}
