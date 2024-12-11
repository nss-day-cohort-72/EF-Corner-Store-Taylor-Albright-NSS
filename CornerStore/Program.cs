using CornerStore.Models;
using CornerStore.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;
using CornerStore.Migrations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// allows passing datetimes without time zone data 
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// allows our api endpoints to access the database through Entity Framework Core and provides dummy value for testing
builder.Services.AddNpgsql<CornerStoreDbContext>(builder.Configuration["CornerStoreDbConnectionString"] ?? "testing");

// Set the JSON serializer options
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/api/cashiers", (CornerStoreDbContext db) => 
{
    List<CashierDTO> cashiersDTO = db.Cashiers
    .Include(c => c.Orders)
    .ThenInclude(o => o.OrderProducts)
    .ThenInclude(op => op.Product)
    .Select(c => new CashierDTO
    {
        Id = c.Id,
        FirstName = c.FirstName,
        LastName = c.LastName,
        Orders = c.Orders.Select(o => new OrderDTO
        {
            Id = o.Id,
            CashierId = o.CashierId,
            PaidOnDate = o.PaidOnDate,
            OrderProducts = o.OrderProducts.Select(op => new OrderProductDTO 
            {
                Id = op.Id,
                ProductId = op.ProductId,
                OrderId = op.OrderId,
                Quantity = op.Quantity,
                Product = new ProductDTO 
                {
                    ProductName = op.Product.ProductName,
                    Price = op.Product.Price
                }
            }).ToList(),
        }).ToList()
    }).ToList();

    return Results.Ok(cashiersDTO);

});

app.MapPost("/api/cashiers", (CornerStoreDbContext db, CashierDTO cashierDTO) => 
{
    Cashier cashier = new Cashier
    {
        FirstName = cashierDTO.FirstName,
        LastName = cashierDTO.LastName,
    };
    db.Cashiers.Add(cashier);
    db.SaveChanges();
    return Results.Created($"/api/cashiers/{cashier.Id}", cashier);
});

app.MapGet("/api/products/", (CornerStoreDbContext db, string search) =>
{
    List<ProductDTO> products = db.Products
        .Include(p => p.Category)
        .Select(p => new ProductDTO
        {
            Id = p.Id,
            ProductName = p.ProductName,
            Price = p.Price,
            Brand = p.Brand,
            CategoryId = p.CategoryId,
            Category = new CategoryDTO
            {
                Id = p.Category.Id,
                CategoryName = p.Category.CategoryName
            }
        }).ToList();
        
    if (search != null)
    {
        products = products.Where(p => p.ProductName.Equals(search, StringComparison.OrdinalIgnoreCase) || p.Category.CategoryName.Equals(search, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    return Results.Ok(products);
});

app.MapPost("/api/products", (CornerStoreDbContext db, ProductDTO productDTO) => 
{
    Product product = new Product
    {
        ProductName = productDTO.ProductName,
        Price = productDTO.Price,
        Brand = productDTO.Brand,
        CategoryId = productDTO.CategoryId,
    };
    db.Products.Add(product);
    db.SaveChanges();
    return Results.Created($"/api/products/{product.Id}", product);
});

app.MapPut("/api/products/{id}", (CornerStoreDbContext db, int id, ProductDTO productDTO) => 
{  
    Product product = db.Products.FirstOrDefault(p => p.Id == id);

    if (product == null)
    {
        return Results.NotFound($"Product with Id of {id} not found");
    }

    product.ProductName = productDTO.ProductName;
    product.Price = productDTO.Price;
    product.Brand = productDTO.Brand;
    product.CategoryId = productDTO.CategoryId;
    db.SaveChanges();
    return Results.Ok($"Product with Id {product.Id} has been changed");
});

app.MapGet("api/orders/", (CornerStoreDbContext db, string orderDate) => 
{
    List<OrderDTO> ordersDTO = db.Orders
    .Include(o => o.Cashier)
    .Include(o => o.OrderProducts)
    .ThenInclude(op => op.Product)
    .Select(o => new OrderDTO 
    {
        Id = o.Id,
        PaidOnDate = o.PaidOnDate,
        CashierId = o.CashierId,
        Cashier = new CashierDTO
        {
            Id = o.Cashier.Id,
            FirstName = o.Cashier.FirstName

        },
        OrderProducts = o.OrderProducts.Select(op => new OrderProductDTO 
        {
            Id = op.Id,
            ProductId = op.ProductId,
            OrderId = op.OrderId,
            Product = new ProductDTO
            {
                Id = op.Product.Id,
                Price = op.Product.Price
            },
            Quantity = op.Quantity

        }).ToList()
    }).ToList();

    if (orderDate != null)
    {
        ordersDTO = ordersDTO.Where(o => o.PaidOnDate.Day == int.Parse(orderDate)).ToList();
    }
    if (ordersDTO.Count == 0)
    {
        return Results.NotFound($"No results were found from your query");
    }
    return Results.Ok(ordersDTO);
});

app.MapGet("/api/orders/{id}", (CornerStoreDbContext db, int id) => 
{
    Order order = db.Orders
    .Include(o => o.Cashier)
    .Include(o => o.OrderProducts)
    .ThenInclude(op => op.Product)
    .FirstOrDefault(o => o.Id == id);

    if (order == null)
    {
        return Results.NotFound("Order not found");
    }

    OrderDTO orderDTO = new OrderDTO
    {
        CashierId = order.CashierId,
        Cashier = new CashierDTO
        {
            FirstName = order.Cashier.FirstName,
            LastName = order.Cashier.LastName
        },
        OrderProducts = order.OrderProducts.Select(op => new OrderProductDTO {
            ProductId = op.ProductId,
            OrderId = op.OrderId,
            Quantity = op.Quantity
        }).ToList()
    };
    return Results.Ok(order);
});

app.MapDelete("/api/orders/{id}", (CornerStoreDbContext db, int id) => 
{
    Order order = db.Orders.FirstOrDefault(o => o.Id == id);
    db.Orders.Remove(order);
    db.SaveChanges();
    return Results.Ok($"Order with Id {id} has been removed");
});

app.MapPost("/api/orders", (CornerStoreDbContext db, OrderDTO orderDTO) =>
{
    Order order = new Order
    {
        CashierId = orderDTO.CashierId,
        PaidOnDate = DateTime.Now,
        OrderProducts = orderDTO.OrderProducts.Select(op => new OrderProduct
        {
            ProductId = op.ProductId,
            OrderId = op.OrderId,
            Product = new Product 
            {
                ProductName = op.Product.ProductName,
                Price = op.Product.Price,
                Brand =  op.Product.Brand,
                CategoryId = op.Product.CategoryId
            },
            Quantity = op.Quantity
        }).ToList()
    };

    db.Orders.Add(order);
    db.SaveChanges();

    return Results.Created($"/api/orders/{order.Id}", order);
});




//Extra handlers
app.MapDelete("/api/product/{id}", (CornerStoreDbContext db, int id) => 
{
    Product product = db.Products.FirstOrDefault(p => p.Id == id);
    if (product == null)
    {
        return Results.NotFound("Could not find a product with that id to delete");
    }

    string productName = product.ProductName;
    db.Products.Remove(product);
    db.SaveChanges();
    return Results.Ok($"{productName} has been deleted");
});

app.Run();

//don't move or change this!
public partial class Program { }