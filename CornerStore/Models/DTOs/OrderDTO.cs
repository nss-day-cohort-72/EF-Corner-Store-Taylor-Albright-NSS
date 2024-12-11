
using System.ComponentModel.DataAnnotations;

namespace CornerStore.Models.DTOs;

public class OrderDTO
{
    public int Id { get; set; }
    [Required]
    public int CashierId { get; set; }
    public CashierDTO Cashier { get; set; }
    //delete cashier if not needed
    public List<OrderProductDTO> OrderProducts { get; set; }

    public decimal Total 
    {
        get
        {
            return OrderProducts.Sum(op => op.Product.Price * op.Quantity);
        }
    }
    public DateTime PaidOnDate { get; set; }
    public List<ProductDTO> Products { get; set; }
}