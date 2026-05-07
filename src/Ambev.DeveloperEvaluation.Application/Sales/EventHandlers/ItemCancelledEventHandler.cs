using Ambev.DeveloperEvaluation.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Sales.EventHandlers;

public sealed class ItemCancelledEventHandler : INotificationHandler<ItemCancelledEvent>
{
    private readonly ILogger<ItemCancelledEventHandler> _logger;

    public ItemCancelledEventHandler(ILogger<ItemCancelledEventHandler> logger) => _logger = logger;

    public Task Handle(ItemCancelledEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "ItemCancelled: SaleId={SaleId}, ItemId={ItemId}, ProductId={ProductId}, Product={ProductName}",
            notification.SaleId, notification.ItemId, notification.ProductId, notification.ProductName);
        return Task.CompletedTask;
    }
}
