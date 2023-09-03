namespace Data.Models.ProductTables;

public class OrderProduct
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public double Price { get; set; }
    public int Amount { get; set; }
    public string Name { get; set; }

    public string OrderId { get; set; }
    public Order Order { get; set; } = default!;

    public string? ProductId { get; set; }
    public Product? Product { get; set; } = default!;

    public OrderProduct(double price, int amount, string name, string productId, string orderId)
    {
        Price = price;
        Amount = amount;
        Name = name;
        ProductId = productId;
        OrderId = orderId;
    }
}