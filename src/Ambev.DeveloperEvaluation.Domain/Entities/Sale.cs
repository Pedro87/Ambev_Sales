using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Exceptions;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

public class Sale : BaseEntity
{
    public string SaleNumber { get; private set; } = string.Empty;
    public DateTime SaleDate { get; private set; }
    public Guid CustomerId { get; private set; }
    public string CustomerName { get; private set; } = string.Empty;
    public Guid BranchId { get; private set; }
    public string BranchName { get; private set; } = string.Empty;
    public decimal TotalAmount { get; private set; }
    public SaleStatus Status { get; private set; }

    private readonly List<SaleItem> _items = [];
    public IReadOnlyCollection<SaleItem> Items => _items.AsReadOnly();

    private Sale() { }

    public Sale(
        string saleNumber,
        DateTime saleDate,
        Guid customerId,
        string customerName,
        Guid branchId,
        string branchName)
    {
        if (string.IsNullOrWhiteSpace(saleNumber))
            throw new DomainException("Sale number is required.");
        if (string.IsNullOrWhiteSpace(customerName))
            throw new DomainException("Customer name is required.");
        if (string.IsNullOrWhiteSpace(branchName))
            throw new DomainException("Branch name is required.");

        SaleNumber = saleNumber;
        SaleDate = saleDate;
        CustomerId = customerId;
        CustomerName = customerName;
        BranchId = branchId;
        BranchName = branchName;
        Status = SaleStatus.Active;

        RaiseDomainEvent(new SaleCreatedEvent(Id, SaleNumber, CustomerId));
    }

    public void AddItem(SaleItem item)
    {
        if (Status == SaleStatus.Cancelled)
            throw new DomainException("Cannot add items to a cancelled sale.");

        var existing = _items.FirstOrDefault(i => i.ProductId == item.ProductId && !i.IsCancelled);
        if (existing is not null)
        {
            var newQty = existing.Quantity + item.Quantity;
            if (newQty > 20)
                throw new DomainException(
                    $"Total quantity for product '{item.ProductName}' would exceed 20. Current: {existing.Quantity}, Adding: {item.Quantity}.");
            existing.UpdateQuantity(newQty);
        }
        else
        {
            item.SetSaleId(Id);
            _items.Add(item);
        }

        RecalculateTotal();
    }

    public void Update(
        string saleNumber,
        DateTime saleDate,
        Guid customerId,
        string customerName,
        Guid branchId,
        string branchName)
    {
        if (Status == SaleStatus.Cancelled)
            throw new DomainException("Cannot update a cancelled sale.");

        SaleNumber = saleNumber;
        SaleDate = saleDate;
        CustomerId = customerId;
        CustomerName = customerName;
        BranchId = branchId;
        BranchName = branchName;

        RecalculateTotal();
        RaiseDomainEvent(new SaleModifiedEvent(Id, SaleNumber));
    }

    public void Cancel()
    {
        if (Status == SaleStatus.Cancelled)
            throw new DomainException("Sale is already cancelled.");

        Status = SaleStatus.Cancelled;
        foreach (var item in _items.Where(i => !i.IsCancelled))
            item.Cancel();

        RaiseDomainEvent(new SaleCancelledEvent(Id, SaleNumber, CustomerId));
    }

    public void CancelItem(Guid itemId)
    {
        if (Status == SaleStatus.Cancelled)
            throw new DomainException("Cannot cancel items of an already cancelled sale.");

        var item = _items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new DomainException($"Item {itemId} not found in sale {Id}.");

        if (item.IsCancelled)
            throw new DomainException($"Item {itemId} is already cancelled.");

        item.Cancel();
        RecalculateTotal();
        RaiseDomainEvent(new ItemCancelledEvent(Id, item.Id, item.ProductId, item.ProductName));
    }

    private void RecalculateTotal() =>
        TotalAmount = _items.Where(i => !i.IsCancelled).Sum(i => i.TotalAmount);
}
