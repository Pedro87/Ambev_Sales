using MediatR;

namespace Ambev.DeveloperEvaluation.Domain.Events;

public sealed record SaleCreatedEvent(Guid SaleId, string SaleNumber, Guid CustomerId) : INotification;
