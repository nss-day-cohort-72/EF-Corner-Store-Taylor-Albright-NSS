using Microsoft.EntityFrameworkCore;
using CornerStore.Models;
public class CornerStoreDbContext : DbContext
{

    public DbSet<Cashier> Cashiers { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderProduct> OrderProducts { get; set; }

    public CornerStoreDbContext(DbContextOptions<CornerStoreDbContext> context) : base(context)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cashier>().HasData(new Cashier[]
        {
            new Cashier {Id = 1, FirstName = "Timothy", LastName = "Bimothy"},
            new Cashier {Id = 2, FirstName = "Sarah", LastName = "Charcuterie"},
            new Cashier {Id = 3, FirstName = "Mandy", LastName = "Blippers"},
            new Cashier {Id = 4, FirstName = "Nathan", LastName = "Nubhands"},
        });

        modelBuilder.Entity<Product>().HasData(new Product[]
        {
            new Product {Id = 1, ProductName = "Hamburger", Price = 10, Brand = "Best Is Best", CategoryId = 1},
            new Product {Id = 2, ProductName = "Salad", Price = 10, Brand = "Slickers", CategoryId = 3},
            new Product {Id = 3, ProductName = "Pizza", Price = 10, Brand = "Pedros", CategoryId = 3},
            new Product {Id = 4, ProductName = "French Fries", Price = 10, Brand = "Hillberry Farms", CategoryId = 3},
        });

        modelBuilder.Entity<Category>().HasData(new Category[]
        {
            new Category {Id = 1, CategoryName = "Main Dish"},
            new Category {Id = 2, CategoryName = "Side"},
            new Category {Id = 3, CategoryName = "Extra"},
        });
        modelBuilder.Entity<Order>().HasData(new Order[]
        {
            new Order {Id = 1, CashierId = 1, PaidOnDate = new DateTime(2024, 12, 10, 12, 30, 0)},
            new Order {Id = 2, CashierId = 2, PaidOnDate = new DateTime(2024, 12, 10, 12, 40, 0)},
            new Order {Id = 3, CashierId = 3, PaidOnDate = new DateTime(2024, 12, 10, 12, 50, 0)},
            new Order {Id = 4, CashierId = 1, PaidOnDate = new DateTime(2024, 12, 10, 1, 10, 0)},
        });
        
        modelBuilder.Entity<OrderProduct>()
        .HasKey(op => new { op.OrderId, op.ProductId });

        modelBuilder.Entity<OrderProduct>()
            .HasOne(op => op.Order)
            .WithMany(o => o.OrderProducts)
            .HasForeignKey(op => op.OrderId);

        modelBuilder.Entity<OrderProduct>()
            .HasOne(op => op.Product)
            .WithMany(p => p.OrderProducts)
            .HasForeignKey(op => op.ProductId);

        modelBuilder.Entity<OrderProduct>().HasData(new OrderProduct[]
        {
            new OrderProduct {Id = 1, ProductId = 1, OrderId = 1, Quantity = 2},
            new OrderProduct {Id = 2, ProductId = 2, OrderId = 1, Quantity = 5},
            new OrderProduct {Id = 3, ProductId = 3, OrderId = 2, Quantity = 10},
            new OrderProduct {Id = 4, ProductId = 2, OrderId = 3, Quantity = 20},
            new OrderProduct {Id = 5, ProductId = 4, OrderId = 4, Quantity = 3},

        });

    }
}