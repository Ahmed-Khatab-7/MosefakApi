namespace MosefakApp.Infrastructure.Identity.context
{
    public class AppIdentityDbContext : IdentityDbContext<AppUser, AppRole, int>
    {

        public AppIdentityDbContext(DbContextOptions<AppIdentityDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfiguration(new AppUserConfig());

            builder.Entity<AppUser>().HasQueryFilter(x => !x.IsDeleted && x.UserType != UserType.Admin);
            builder.Entity<AppRole>().HasQueryFilter(x => !x.IsDeleted);

            builder.Entity<AppUser>().ToTable(name: "Users", schema: "Security");
            builder.Entity<IdentityUserRole<int>>().ToTable(name: "UserRoles", schema: "Security");
            builder.Entity<AppRole>().ToTable(name: "Roles", schema: "Security");
            builder.Entity<IdentityRoleClaim<int>>().ToTable(name: "RoleClaims", schema: "Security");
            builder.Entity<IdentityUserRole<int>>().ToTable(name: "UserRoles", schema: "Security");
        }
    }
    //public class BloggingContextFactory : IDesignTimeDbContextFactory<AppIdentityDbContext>
    //{
    //    public AppIdentityDbContext CreateDbContext(string[] args)
    //    {
    //        var optionsBuilder = new DbContextOptionsBuilder<AppIdentityDbContext>();


    //        optionsBuilder.UseSqlServer("server=.; database=MosefakManagement; Integrated Security=SSPI; trustServerCertificate=true;");


    //        return new AppIdentityDbContext(optionsBuilder.Options);
    //    }

        
    //}
}
