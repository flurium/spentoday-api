namespace Data.Models.ProductTables;

public class Order
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string Email { get; set; }

    public string ProductId { get; set; }
    public Product Product { get; set; }

    public Order(string email, string productId)
    {
        this.Email = email;
        this.ProductId = productId;
    }
}