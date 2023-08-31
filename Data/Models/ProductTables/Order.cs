namespace Data.Models.ProductTables;

public class Order
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string Email { get; set; }
    public string Adress { get; set; }
    public string FullName { get; set; }
    public string Comment { get; set; }

    public string PostIndex { get; set; }

    public IReadOnlyCollection<OrderProduct> OrderProducts { get; set; } = default!;

    public Order(string email, string adress, string fullName, string postIndex, string comment)
    {
        Email = email;
        Adress = adress;
        Comment = comment;
        FullName = fullName;
        PostIndex = postIndex;
    }
}