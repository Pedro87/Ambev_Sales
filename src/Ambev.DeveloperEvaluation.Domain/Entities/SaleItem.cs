using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Exceptions;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

public class SaleItem : BaseEntity
{
    public Guid SaleId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal DiscountPercentage { get; private set; }
    public decimal TotalAmount { get; private set; }
    public bool IsCancelled { get; private set; }

    private SaleItem() { }

    public SaleItem(Guid productId, string productName, int quantity, decimal unitPrice)
    {
        if (quantity > 20)
            throw new DomainException($"Cannot add more than 20 identical items. Requested: {quantity}.");
        if (string.IsNullOrWhiteSpace(productName))
            throw new DomainException("Product name is required.");
        if (unitPrice <= 0)
            throw new DomainException("Unit price must be greater than zero.");
        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than zero.");

        ProductId = productId;
        ProductName = productName;
        Quantity = quantity;
        UnitPrice = unitPrice;
        IsCancelled = false;
        ApplyDiscount();
    }

    private void ApplyDiscount()
    {
        DiscountPercentage = Quantity switch
        {
            >= 10 => 0.20m,
            >= 4  => 0.10m,
            _     => 0.00m
        };
        TotalAmount = Quantity * UnitPrice * (1 - DiscountPercentage);
    }

    public void UpdateQuantity(int newQuantity)
    {
        if (newQuantity > 20)
            throw new DomainException($"Cannot set more than 20 identical items. Requested: {newQuantity}.");
        if (newQuantity <= 0)
            throw new DomainException("Quantity must be greater than zero.");
        Quantity = newQuantity;
        ApplyDiscount();
    }

    public void Cancel() => IsCancelled = true;

    internal void SetSaleId(Guid saleId) => SaleId = saleId;
}
