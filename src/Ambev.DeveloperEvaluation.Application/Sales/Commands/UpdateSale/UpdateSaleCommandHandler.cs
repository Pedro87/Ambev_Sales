using Ambev.DeveloperEvaluation.Application.Sales.DTOs;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.Commands.UpdateSale;

public sealed class UpdateSaleCommandHandler : IRequestHandler<UpdateSaleCommand, SaleDto>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly IPublisher _publisher;

    public UpdateSaleCommandHandler(ISaleRepository saleRepository, IMapper mapper, IPublisher publisher)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
        _publisher = publisher;
    }

    public async Task<SaleDto> Handle(UpdateSaleCommand request, CancellationToken cancellationToken)
    {
        var sale = await _saleRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new DomainException($"Sale {request.Id} not found.");

        sale.Update(
            request.SaleNumber,
            request.SaleDate,
            request.CustomerId,
            request.CustomerName,
            request.BranchId,
            request.BranchName);

        foreach (var existingItem in sale.Items.Where(i => !i.IsCancelled).ToList())
            sale.CancelItem(existingItem.Id);

        foreach (var itemRequest in request.Items)
            sale.AddItem(new SaleItem(
                itemRequest.ProductId,
                itemRequest.ProductName,
                itemRequest.Quantity,
                itemRequest.UnitPrice));

        var updated = await _saleRepository.UpdateAsync(sale, cancellationToken);

        foreach (var domainEvent in updated.DomainEvents)
            await _publisher.Publish(domainEvent, cancellationToken);
        updated.ClearDomainEvents();

        return _mapper.Map<SaleDto>(updated);
    }
}
