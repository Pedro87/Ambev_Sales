namespace Ambev.DeveloperEvaluation.Application.Sales.DTOs;

public sealed class UpdateSaleRequest
{
    public string SaleNumber { get; init; } = string.Empty;
    public DateTime SaleDate { get; init; }
    public Guid CustomerId { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public Guid BranchId { get; init; }
    public string BranchName { get; init; } = string.Empty;
    public List<CreateSaleItemRequest> Items { get; init; } = [];
}
