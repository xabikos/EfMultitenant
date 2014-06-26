using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Multitenant.Interception.Entities;

namespace Multitenant.Interception.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser, IPrincipal
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class CustonIdentity : IIdentity
    {
        public string Name { get; private set; }
        public string AuthenticationType { get; private set; }
        public bool IsAuthenticated { get; private set; }


    }

    public class CustomPrincipal : IPrincipal
    {

        

        public CustomPrincipal(CustonIdentity identity)
        {
            Identity = identity;
        }

        public bool IsInRole(string role)
        {
            var customIdentity = (CustonIdentity) Identity;


        }

        public IIdentity Identity { get; private set; }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        public DbSet<ProductCategory> Categories { get; set; }

        public DbSet<Product> Products { get; set; }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ProductCategory>()
                .HasRequired(pc => pc.User)
                .WithMany()
                .HasForeignKey(pc => pc.TenantId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Product>()
                .HasRequired(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.TenantId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<Product>()
                .HasRequired(p => p.Category)
                .WithMany()
                .HasForeignKey(p => p.CategoryId)
                .WillCascadeOnDelete(false);
            
            var conv = new AttributeToTableAnnotationConvention<TenantAttribute, string>(
                "TenantColumnName",
                (type, attributes) => attributes.Single().ColumnName);

            modelBuilder.Conventions.Add(conv);

        }

    }
}