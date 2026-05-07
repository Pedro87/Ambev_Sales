using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.Commands.CancelSaleItem;

public sealed class CancelSaleItemCommand : IRequest<bool>
{
    public Guid SaleId { get; init; }
    public Guid ItemId { get; init; }
}
