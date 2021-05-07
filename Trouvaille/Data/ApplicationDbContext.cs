using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using AuthoDemoMVC.Models;
using Trouvaille.Models;
using Trouvaille_WebAPI.Models;

namespace AuthoDemoMVC.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        
        public DbSet<Address> Address { get; set; }
        public DbSet<Category> Category { get; set; }
        public DbSet<City> City { get; set; }
        public DbSet<CompanyData> CompanyData { get; set; }
        public DbSet<Order> Order { get; set; }
        public DbSet<Picture> Picture { get; set; }
        public DbSet<Product> Product { get; set; }
        public DbSet<Rating> Rating { get; set; }
        public DbSet<Manufacturer> Manufacturer { get; set; }
        //public DbSet<ApplicationUser> ApplicationUser { get; set; }
        //public DbSet<CategoryProduct> CategoryProduct { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Category>(entity => {
                entity.HasIndex(e => e.Name).IsUnique();
            });
            /**
            builder.Entity<CategoryProduct>(entity =>
                entity.HasKey(e => new {e.ProductCategoriesCategoryId, e.ProductsProductId}));
            **/
        }
    }
}
