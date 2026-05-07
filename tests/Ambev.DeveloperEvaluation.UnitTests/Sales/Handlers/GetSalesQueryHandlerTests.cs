using Ambev.DeveloperEvaluation.Application.Sales.Mapping;
using Ambev.DeveloperEvaluation.Application.Sales.Queries.GetSales;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.UnitTests.Sales.TestData;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.UnitTests.Sales.Handlers;

public sealed class GetSalesQueryHandlerTests
{
    private readonly ISaleRepository _repository;
    private readonly IMapper _mapper;
    private readonly GetSalesQueryHandler _handler;

    public GetSalesQueryHandlerTests()
    {
        _repository = Substitute.For<ISaleRepository>();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAutoMapper(cfg => cfg.AddProfile<SaleProfile>());
        _mapper = services.BuildServiceProvider().GetRequiredService<IMapper>();

        _handler = new GetSalesQueryHandler(_repository, _mapper);
    }

    [Fact]
    public async Task Handle_WithSales_ShouldReturnPaginatedResponse()
    {
        var sales = new List<Sale>
        {
            SaleTestData.CreateValidSale(),
            SaleTestData.CreateValidSale()
        };

        _repository
            .GetPagedAsync(1, 10, null, null, null, null, Arg.Any<CancellationToken>())
            .Returns((sales.AsEnumerable(), 2));

        var result = await _handler.Handle(new GetSalesQuery { Page = 1, Size = 10 }, CancellationToken.None);

        result.Should().NotBeNull();
        result.Data.Should().HaveCount(2);
        result.TotalItems.Should().Be(2);
        result.CurrentPage.Should().Be(1);
        result.TotalPages.Should().Be(1);
    }

    [Fact]
    public async Task Handle_EmptyRepository_ShouldReturnEmptyResponse()
    {
        _repository
            .GetPagedAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(),
                Arg.Any<Guid?>(), Arg.Any<Guid?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns((Enumerable.Empty<Sale>(), 0));

        var result = await _handler.Handle(new GetSalesQuery(), CancellationToken.None);

        result.Data.Should().BeEmpty();
        result.TotalItems.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }
}
