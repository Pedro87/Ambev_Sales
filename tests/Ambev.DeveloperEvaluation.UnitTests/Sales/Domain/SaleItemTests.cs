using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.UnitTests.Sales.TestData;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.UnitTests.Sales.Domain;

public sealed class SaleItemTests
{
    [Theory]
    [InlineData(1, 0.00)]
    [InlineData(3, 0.00)]
    [InlineData(4, 0.10)]
    [InlineData(9, 0.10)]
    [InlineData(10, 0.20)]
    [InlineData(20, 0.20)]
    public void Constructor_ShouldApplyCorrectDiscount(int quantity, decimal expectedDiscount)
    {
        var item = SaleTestData.CreateSaleItemWithQuantity(quantity);

        item.DiscountPercentage.Should().Be(expectedDiscount);
        item.TotalAmount.Should().Be(quantity * 100m * (1 - expectedDiscount));
    }

    [Fact]
    public void Constructor_QuantityAbove20_ShouldThrowDomainException()
    {
        var act = () => SaleTestData.CreateSaleItemWithQuantity(21);

        act.Should().Throw<DomainException>().WithMessage("*20*");
    }

    [Fact]
    public void Constructor_ZeroQuantity_ShouldThrowDomainException()
    {
        var act = () => new SaleItem(Guid.NewGuid(), "Product", 0, 100m);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Constructor_ZeroUnitPrice_ShouldThrowDomainException()
    {
        var act = () => new SaleItem(Guid.NewGuid(), "Product", 1, 0m);
     
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void UpdateQuantity_ValidQuantity_ShouldRecalculateDiscount()
    {
        var item = SaleTestData.CreateSaleItemWithQuantity(3);

        item.UpdateQuantity(10);

        item.Quantity.Should().Be(10);
        item.DiscountPercentage.Should().Be(0.20m);
        item.TotalAmount.Should().Be(10 * 100m * 0.80m);
    }

    [Fact]
    public void Cancel_ShouldSetIsCancelledTrue()
    {
        var item = SaleTestData.CreateSaleItemWithQuantity(2);
        item.Cancel();
        item.IsCancelled.Should().BeTrue();
    }
}
