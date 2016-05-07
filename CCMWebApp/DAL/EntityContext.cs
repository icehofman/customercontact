using CCMWebApp.Models;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace CCMWebApp.DAL
{
    public class EntityContext : DbContext
    {
        public virtual DbSet<Region> Region { get; set; }
        public virtual DbSet<User> User { get; set; }
        public virtual DbSet<City> City { get; set; }
        public virtual DbSet<Classification> Classification { get; set; }
        public virtual DbSet<Client> Client { get; set; }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
    }

}
