using Ambev.DeveloperEvaluation.Application.Sales.Commands.CancelSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.UnitTests.Sales.TestData;
using FluentAssertions;
using MediatR;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.UnitTests.Sales.Handlers;

public sealed class CancelSaleCommandHandlerTests
{
    private readonly ISaleRepository _repository;
    private readonly IPublisher _publisher;
    private readonly CancelSaleCommandHandler _handler;

    public CancelSaleCommandHandlerTests()
    {
        _repository = Substitute.For<ISaleRepository>();
        _publisher = Substitute.For<IPublisher>();
        _handler = new CancelSaleCommandHandler(_repository, _publisher);
    }

    [Fact]
    public async Task Handle_ExistingSale_ShouldCancelAndReturnTrue()
    {
        var sale = SaleTestData.CreateValidSale();

        _repository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);
        _repository.UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>()).Returns(sale);

        var result = await _handler.Handle(new CancelSaleCommand { Id = sale.Id }, CancellationToken.None);

        result.Should().BeTrue();
        sale.Status.Should().Be(SaleStatus.Cancelled);
        await _repository.Received(1).UpdateAsync(sale, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NonExistingSale_ShouldThrowDomainException()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Sale?)null);

        var act = async () => await _handler.Handle(
            new CancelSaleCommand { Id = Guid.NewGuid() }, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*not found*");
    }
}
