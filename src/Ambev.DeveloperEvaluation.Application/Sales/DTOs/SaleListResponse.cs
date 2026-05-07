namespace Ambev.DeveloperEvaluation.Application.Sales.DTOs;

public sealed class SaleListResponse
{
    public IEnumerable<SaleDto> Data { get; init; } = [];
    public int TotalItems { get; init; }
    public int CurrentPage { get; init; }
    public int TotalPages { get; init; }
}
