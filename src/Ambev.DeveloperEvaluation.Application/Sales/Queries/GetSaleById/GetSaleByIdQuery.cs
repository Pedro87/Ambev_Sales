using Ambev.DeveloperEvaluation.Application.Sales.DTOs;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.Queries.GetSaleById;

public sealed class GetSaleByIdQuery : IRequest<SaleDto>
{
    public Guid Id { get; init; }
}
