using Ambev.DeveloperEvaluation.Domain.Entities;
using Bogus;

namespace Ambev.DeveloperEvaluation.UnitTests.Sales.TestData;

public static class SaleTestData
{
    private static readonly Faker _faker = new("pt_BR");

    public static Sale CreateValidSale()
    {
        var sale = new Sale(
            _faker.Random.AlphaNumeric(10).ToUpper(),
            DateTime.UtcNow,
            Guid.NewGuid(),
            _faker.Company.CompanyName(),
            Guid.NewGuid(),
            _faker.Address.City());

        sale.AddItem(CreateValidSaleItem());
        return sale;
    }

    public static SaleItem CreateValidSaleItem(int quantity = 1) =>
        new(Guid.NewGuid(), _faker.Commerce.ProductName(), quantity, _faker.Random.Decimal(1, 500));

    public static SaleItem CreateSaleItemWithQuantity(int quantity) =>
        new(Guid.NewGuid(), _faker.Commerce.ProductName(), quantity, 100m);
}
