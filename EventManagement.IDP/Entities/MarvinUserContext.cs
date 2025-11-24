using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Marvin.IDP.Entities
{
    public class MarvinUserContext : IdentityDbContext<IdentityUser>
    {
        public MarvinUserContext(DbContextOptions<MarvinUserContext> options)
           : base(options)
        {
           
        }

    }

    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    modelBuilder.Entity< Microsoft.AspNetCore.Identity.IdentityUserClaim<string>>()
        //                .Property(p => p.Id)
        //                .ValueGeneratedOnAdd();
        //}
    }

    public class ApplicationAccountDbContext : DbContext
    {
        public ApplicationAccountDbContext(DbContextOptions<ApplicationAccountDbContext> options) : base(options) { }
        public virtual DbSet<Edge_Account> Edge_Accounts { get; set; }
        public virtual DbSet<Edge_Client> Edge_Clients { get; set; }
        public virtual DbSet<Edge_ClientUser> Edge_ClientUsers { get; set; }
        public virtual DbSet<Edge_UserAccount> Edge_UserAccounts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("Server=LAPTOP-A86D8B2F;Database=eintelligence;Trusted_Connection=True;");
            }
        }
      

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    modelBuilder.Entity< Microsoft.AspNetCore.Identity.IdentityUserClaim<string>>()
        //                .Property(p => p.Id)
        //                .ValueGeneratedOnAdd();
        //}
    }
}
