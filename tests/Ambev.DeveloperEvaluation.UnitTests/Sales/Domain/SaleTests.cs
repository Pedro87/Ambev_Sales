using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.UnitTests.Sales.TestData;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.UnitTests.Sales.Domain;

public sealed class SaleTests
{
    [Fact]
    public void Constructor_ValidArguments_ShouldCreateSaleWithActiveStatus()
    {
        var sale = SaleTestData.CreateValidSale();

        sale.Status.Should().Be(SaleStatus.Active);
        sale.Id.Should().NotBeEmpty();
        sale.DomainEvents.Should().ContainSingle(e => e is SaleCreatedEvent);
    }

    [Fact]
    public void Constructor_EmptySaleNumber_ShouldThrowDomainException()
    {
        var act = () => new Sale(string.Empty, DateTime.UtcNow, Guid.NewGuid(), "Customer", Guid.NewGuid(), "Branch");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void AddItem_SameProductTwice_ShouldMergeQuantities()
    {
        var sale = new Sale("S001", DateTime.UtcNow, Guid.NewGuid(), "Customer", Guid.NewGuid(), "Branch");
        var productId = Guid.NewGuid();

        sale.AddItem(new SaleItem(productId, "Beer", 3, 10m));
        sale.AddItem(new SaleItem(productId, "Beer", 4, 10m));

        sale.Items.Should().HaveCount(1);
        sale.Items.Single().Quantity.Should().Be(7);
        sale.Items.Single().DiscountPercentage.Should().Be(0.10m);
    }

    [Fact]
    public void AddItem_MergingExceedsLimit_ShouldThrowDomainException()
    {
        var sale = new Sale("S001", DateTime.UtcNow, Guid.NewGuid(), "Customer", Guid.NewGuid(), "Branch");
        var productId = Guid.NewGuid();

        sale.AddItem(new SaleItem(productId, "Beer", 15, 10m));

        var act = () => sale.AddItem(new SaleItem(productId, "Beer", 10, 10m));

        act.Should().Throw<DomainException>().WithMessage("*exceed 20*");
    }

    [Fact]
    public void AddItem_ToCancelledSale_ShouldThrowDomainException()
    {
        var sale = SaleTestData.CreateValidSale();
        sale.Cancel();

        var act = () => sale.AddItem(SaleTestData.CreateValidSaleItem());

        act.Should().Throw<DomainException>().WithMessage("*cancelled*");
    }

    [Fact]
    public void Cancel_ActiveSale_ShouldSetStatusCancelledAndRaiseEvent()
    {
        var sale = SaleTestData.CreateValidSale();
        sale.ClearDomainEvents();

        sale.Cancel();

        sale.Status.Should().Be(SaleStatus.Cancelled);
        sale.Items.Should().AllSatisfy(i => i.IsCancelled.Should().BeTrue());
        sale.DomainEvents.Should().ContainSingle(e => e is SaleCancelledEvent);
    }

    [Fact]
    public void Cancel_AlreadyCancelledSale_ShouldThrowDomainException()
    {
        var sale = SaleTestData.CreateValidSale();
        sale.Cancel();

        var act = () => sale.Cancel();

        act.Should().Throw<DomainException>().WithMessage("*already cancelled*");
    }

    [Fact]
    public void CancelItem_ValidItem_ShouldCancelItemAndRaiseEvent()
    {
        var sale = SaleTestData.CreateValidSale();
        var itemId = sale.Items.First().Id;
        sale.ClearDomainEvents();

        sale.CancelItem(itemId);

        sale.Items.Single(i => i.Id == itemId).IsCancelled.Should().BeTrue();
        sale.DomainEvents.Should().ContainSingle(e => e is ItemCancelledEvent);
    }

    [Fact]
    public void CancelItem_NotExistingItem_ShouldThrowDomainException()
    {
        var sale = SaleTestData.CreateValidSale();

        var act = () => sale.CancelItem(Guid.NewGuid());

        act.Should().Throw<DomainException>().WithMessage("*not found*");
    }

    [Fact]
    public void TotalAmount_ShouldExcludeCancelledItems()
    {
        var sale = new Sale("S001", DateTime.UtcNow, Guid.NewGuid(), "Customer", Guid.NewGuid(), "Branch");
        var item1 = new SaleItem(Guid.NewGuid(), "Product A", 2, 100m);
        var item2 = new SaleItem(Guid.NewGuid(), "Product B", 1, 50m);

        sale.AddItem(item1);
        sale.AddItem(item2);
        sale.CancelItem(item2.Id);

        sale.TotalAmount.Should().Be(200m);
    }

    [Fact]
    public void Update_CancelledSale_ShouldThrowDomainException()
    {
        var sale = SaleTestData.CreateValidSale();
        sale.Cancel();

        var act = () => sale.Update("NEW", DateTime.UtcNow, Guid.NewGuid(), "C", Guid.NewGuid(), "B");

        act.Should().Throw<DomainException>().WithMessage("*cancelled*");
    }
}
