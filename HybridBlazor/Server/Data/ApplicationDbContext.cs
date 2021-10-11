using System;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Duende.IdentityServer.EntityFramework.Options;
using HybridBlazor.Server.Data.Models;

namespace HybridBlazor.Server.Data
{
    public class ApplicationDbContext : ApiAuthorizationDbContext<ApplicationUser>
    {
        public ApplicationDbContext(
            DbContextOptions options,
            IOptions<OperationalStoreOptions> operationalStoreOptions) : base(options, operationalStoreOptions)
        {
        }

        public DbSet<Counter> Counters { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            var user = new ApplicationUser
            {
                Id = "9c6529c1-edba-4077-b68a-ddaac0429a5d",
                Email = "demo",
                UserName = "demo",
                NormalizedUserName = "DEMO",
                NormalizedEmail = "DEMO",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
            };
            user.PasswordHash = new PasswordHasher<ApplicationUser>().HashPassword(user, "demo");

            builder.Entity<ApplicationUser>().HasData(user);

            base.OnModelCreating(builder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLazyLoadingProxies();
        }
    }
}
