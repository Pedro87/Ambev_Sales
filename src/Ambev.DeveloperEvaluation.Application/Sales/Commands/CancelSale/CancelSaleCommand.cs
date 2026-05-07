using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.Commands.CancelSale;

public sealed class CancelSaleCommand : IRequest<bool>
{
    public Guid Id { get; init; }
}
