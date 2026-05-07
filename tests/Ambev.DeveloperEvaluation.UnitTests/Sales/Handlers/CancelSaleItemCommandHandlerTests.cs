using Ambev.DeveloperEvaluation.Application.Sales.Commands.CancelSaleItem;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.UnitTests.Sales.TestData;
using FluentAssertions;
using MediatR;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.UnitTests.Sales.Handlers;

public sealed class CancelSaleItemCommandHandlerTests
{
    private readonly ISaleRepository _repository;
    private readonly IPublisher _publisher;
    private readonly CancelSaleItemCommandHandler _handler;

    public CancelSaleItemCommandHandlerTests()
    {
        _repository = Substitute.For<ISaleRepository>();
        _publisher = Substitute.For<IPublisher>();
        _handler = new CancelSaleItemCommandHandler(_repository, _publisher);
    }

    [Fact]
    public async Task Handle_ValidItem_ShouldCancelItemAndReturnTrue()
    {
        var sale = SaleTestData.CreateValidSale();
        var itemId = sale.Items.First().Id;

        _repository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);
        _repository.UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>()).Returns(sale);

        var result = await _handler.Handle(
            new CancelSaleItemCommand { SaleId = sale.Id, ItemId = itemId },
            CancellationToken.None);

        result.Should().BeTrue();
        sale.Items.Single(i => i.Id == itemId).IsCancelled.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_SaleNotFound_ShouldThrowDomainException()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Sale?)null);

        var act = async () => await _handler.Handle(
            new CancelSaleItemCommand { SaleId = Guid.NewGuid(), ItemId = Guid.NewGuid() },
            CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }
}
