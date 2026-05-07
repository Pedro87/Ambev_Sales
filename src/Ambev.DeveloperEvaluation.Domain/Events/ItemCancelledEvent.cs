using MediatR;

namespace Ambev.DeveloperEvaluation.Domain.Events;

public sealed record ItemCancelledEvent(Guid SaleId, Guid ItemId, Guid ProductId, string ProductName) : INotification;
