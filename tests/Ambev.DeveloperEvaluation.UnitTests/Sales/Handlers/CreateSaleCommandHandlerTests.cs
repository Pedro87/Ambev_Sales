using Ambev.DeveloperEvaluation.Application.Sales.Commands.CreateSale;
using Ambev.DeveloperEvaluation.Application.Sales.DTOs;
using Ambev.DeveloperEvaluation.Application.Sales.Mapping;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Bogus;
using FluentAssertions;
using MediatR;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.UnitTests.Sales.Handlers;

public sealed class CreateSaleCommandHandlerTests
{
    private readonly ISaleRepository _repository;
    private readonly IMapper _mapper;
    private readonly IPublisher _publisher;
    private readonly CreateSaleCommandHandler _handler;
    private readonly Faker _faker = new("pt_BR");

    public CreateSaleCommandHandlerTests()
    {
        _repository = Substitute.For<ISaleRepository>();
        _publisher = Substitute.For<IPublisher>();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAutoMapper(cfg => cfg.AddProfile<SaleProfile>());
        _mapper = services.BuildServiceProvider().GetRequiredService<IMapper>();

        _handler = new CreateSaleCommandHandler(_repository, _mapper, _publisher);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateSaleAndReturnDto()
    {
        var command = BuildValidCommand(quantity: 5);

        _repository
            .CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(args => args.Arg<Sale>());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.SaleNumber.Should().Be(command.SaleNumber);
        result.Items.Should().HaveCount(1);
        result.Items[0].DiscountPercentage.Should().Be(0.10m);

        await _repository.Received(1).CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
        await _publisher.Received().Publish(Arg.Any<INotification>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ItemWith10Quantity_ShouldApply20PercentDiscount()
    {
        var command = BuildValidCommand(quantity: 10);

        _repository
            .CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(args => args.Arg<Sale>());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Items[0].DiscountPercentage.Should().Be(0.20m);
        result.Items[0].TotalAmount.Should().Be(10 * 100m * 0.80m);
    }

    [Fact]
    public async Task Handle_ItemWithQuantityAbove20_ShouldThrowDomainException()
    {
        var command = BuildValidCommand(quantity: 21);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    private CreateSaleCommand BuildValidCommand(int quantity = 1) => new()
    {
        SaleNumber = _faker.Random.AlphaNumeric(10),
        SaleDate = DateTime.UtcNow,
        CustomerId = Guid.NewGuid(),
        CustomerName = _faker.Company.CompanyName(),
        BranchId = Guid.NewGuid(),
        BranchName = _faker.Address.City(),
        Items =
        [
            new CreateSaleItemRequest
            {
                ProductId = Guid.NewGuid(),
                ProductName = _faker.Commerce.ProductName(),
                Quantity = quantity,
                UnitPrice = 100m
            }
        ]
    };
}
