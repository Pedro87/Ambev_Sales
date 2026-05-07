using Ambev.DeveloperEvaluation.Application.Sales.DTOs;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.Queries.GetSales;

public sealed class GetSalesQuery : IRequest<SaleListResponse>
{
    public int Page { get; init; } = 1;
    public int Size { get; init; } = 10;
    public string? Order { get; init; }
    public Guid? CustomerId { get; init; }
    public Guid? BranchId { get; init; }
    public string? Status { get; init; }
}
