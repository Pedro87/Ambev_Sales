namespace Ambev.DeveloperEvaluation.Application.Sales.DTOs;

public sealed class CreateSaleItemRequest
{
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
}
