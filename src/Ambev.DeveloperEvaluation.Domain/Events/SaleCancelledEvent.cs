using MediatR;

namespace Ambev.DeveloperEvaluation.Domain.Events;

public sealed record SaleCancelledEvent(Guid SaleId, string SaleNumber, Guid CustomerId) : INotification;
