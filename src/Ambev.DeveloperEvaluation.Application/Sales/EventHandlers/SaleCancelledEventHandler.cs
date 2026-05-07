using Ambev.DeveloperEvaluation.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Sales.EventHandlers;

public sealed class SaleCancelledEventHandler : INotificationHandler<SaleCancelledEvent>
{
    private readonly ILogger<SaleCancelledEventHandler> _logger;

    public SaleCancelledEventHandler(ILogger<SaleCancelledEventHandler> logger) => _logger = logger;

    public Task Handle(SaleCancelledEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "SaleCancelled: SaleId={SaleId}, SaleNumber={SaleNumber}, CustomerId={CustomerId}",
            notification.SaleId, notification.SaleNumber, notification.CustomerId);
        return Task.CompletedTask;
    }
}
