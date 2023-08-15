using BulkyBook.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BulkyBook.DataAccess
{
    public class ApplicationDBContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options) { }
        public DbSet<CategoryModel> Categories { get; set; } // table
        public DbSet<CoverTypeModel> CoverType { get; set; } // table
        public DbSet<ProductModel> Product { get; set; } // table
        public DbSet<ApplicationUserModel> ApplicationUser { get; set; } // table
        public DbSet<CompanyModel> Company { get; set; } // table
        public DbSet<ShoppingCartModel> ShoppingCart { get; set; } // table
        public DbSet<OrderHeaderModel> OrderHeader { get; set; } // table
        public DbSet<OrderDetailsModel> OrderDetails { get; set; } // table
        public DbSet<ProductImageModel> ProductImages { get; set; } // table

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<CategoryModel>().HasData(
                    new CategoryModel { Id = 1, Name = "Action", DisplayOrder = 1, DateTimeCreated = DateTime.Now },
                    new CategoryModel { Id = 2, Name = "Horror", DisplayOrder = 2, DateTimeCreated = DateTime.Now },
                    new CategoryModel { Id = 3, Name = "SciFi", DisplayOrder = 3, DateTimeCreated = DateTime.Now }
                );

            modelBuilder.Entity<CoverTypeModel>().HasData(
                    new CoverTypeModel { Id = 1, Name = "Hard Cover" },
                    new CoverTypeModel { Id = 2, Name = "Soft Cover" }
                );


            modelBuilder.Entity<ProductModel>().HasData(
                    new ProductModel
                    {
                        Id = 1,
                        Title = "Fortune of Time",
                        Author = "Billy Spark",
                        Description = "Praesent vitae sodales libero. Praesent molestie orci augue, vitae euismod velit sollicitudin ac. Praesent vestibulum facilisis nibh ut ultricies.\r\n\r\nNunc malesuada viverra ipsum sit amet tincidunt. ",
                        ISBN = "SWD9999001",
                        ListPrice = 99,
                        Price = 90,
                        Price_50 = 85,
                        Price_100 = 80,
                        CategoryId = 1,
                        CoverTypeId = 1
                    },
                    new ProductModel
                    {
                        Id = 2,
                        Title = "Dark Skies",
                        Author = "Nancy Hoover",
                        Description = "Praesent vitae sodales libero. Praesent molestie orci augue, vitae euismod velit sollicitudin ac. Praesent vestibulum facilisis nibh ut ultricies.\r\n\r\nNunc malesuada viverra ipsum sit amet tincidunt. ",
                        ISBN = "CAW777777701",
                        ListPrice = 40,
                        Price = 30,
                        Price_50 = 25,
                        Price_100 = 20,
                        CategoryId = 1,
                        CoverTypeId = 2
                    },
                    new ProductModel
                    {
                        Id = 3,
                        Title = "Vanish in the Sunset",
                        Author = "Julian Button",
                        Description = "Praesent vitae sodales libero. Praesent molestie orci augue, vitae euismod velit sollicitudin ac. Praesent vestibulum facilisis nibh ut ultricies.\r\n\r\nNunc malesuada viverra ipsum sit amet tincidunt. ",
                        ISBN = "RITO5555501",
                        ListPrice = 55,
                        Price = 50,
                        Price_50 = 40,
                        Price_100 = 35,
                        CategoryId = 2,
                        CoverTypeId = 1
                    },
                    new ProductModel
                    {
                        Id = 4,
                        Title = "Cotton Candy",
                        Author = "Abby Muscles",
                        Description = "Praesent vitae sodales libero. Praesent molestie orci augue, vitae euismod velit sollicitudin ac. Praesent vestibulum facilisis nibh ut ultricies.\r\n\r\nNunc malesuada viverra ipsum sit amet tincidunt. ",
                        ISBN = "WS3333333301",
                        ListPrice = 70,
                        Price = 65,
                        Price_50 = 60,
                        Price_100 = 55,
                        CategoryId = 3,
                        CoverTypeId = 2
                    },
                    new ProductModel
                    {
                        Id = 5,
                        Title = "Rock in the Ocean",
                        Author = "Ron Parker",
                        Description = "Praesent vitae sodales libero. Praesent molestie orci augue, vitae euismod velit sollicitudin ac. Praesent vestibulum facilisis nibh ut ultricies.\r\n\r\nNunc malesuada viverra ipsum sit amet tincidunt. ",
                        ISBN = "SOTJ1111111101",
                        ListPrice = 30,
                        Price = 27,
                        Price_50 = 25,
                        Price_100 = 20,
                        CategoryId = 2,
                        CoverTypeId = 1
                    },
                    new ProductModel
                    {
                        Id = 6,
                        Title = "Leaves and Wonders",
                        Author = "Laura Phantom",
                        Description = "Praesent vitae sodales libero. Praesent molestie orci augue, vitae euismod velit sollicitudin ac. Praesent vestibulum facilisis nibh ut ultricies.\r\n\r\nNunc malesuada viverra ipsum sit amet tincidunt. ",
                        ISBN = "FOT000000001",
                        ListPrice = 25,
                        Price = 23,
                        Price_50 = 22,
                        Price_100 = 20,
                        CategoryId = 3,
                        CoverTypeId = 2
                    }
                );
        }
    }
}
