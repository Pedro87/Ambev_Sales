using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.Commands.CancelSaleItem;

public sealed class CancelSaleItemCommandHandler : IRequestHandler<CancelSaleItemCommand, bool>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IPublisher _publisher;

    public CancelSaleItemCommandHandler(ISaleRepository saleRepository, IPublisher publisher)
    {
        _saleRepository = saleRepository;
        _publisher = publisher;
    }

    public async Task<bool> Handle(CancelSaleItemCommand request, CancellationToken cancellationToken)
    {
        var sale = await _saleRepository.GetByIdAsync(request.SaleId, cancellationToken)
            ?? throw new DomainException($"Sale {request.SaleId} not found.");

        sale.CancelItem(request.ItemId);
        await _saleRepository.UpdateAsync(sale, cancellationToken);

        foreach (var domainEvent in sale.DomainEvents)
            await _publisher.Publish(domainEvent, cancellationToken);
        sale.ClearDomainEvents();

        return true;
    }
}
