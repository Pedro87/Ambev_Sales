using Ambev.DeveloperEvaluation.Application.Sales.DTOs;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.Queries.GetSales;

public sealed class GetSalesQueryHandler : IRequestHandler<GetSalesQuery, SaleListResponse>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;

    public GetSalesQueryHandler(ISaleRepository saleRepository, IMapper mapper)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
    }

    public async Task<SaleListResponse> Handle(GetSalesQuery request, CancellationToken cancellationToken)
    {
        var (sales, totalCount) = await _saleRepository.GetPagedAsync(
            request.Page,
            request.Size,
            request.Order,
            request.CustomerId,
            request.BranchId,
            request.Status,
            cancellationToken);

        return new SaleListResponse
        {
            Data = _mapper.Map<IEnumerable<SaleDto>>(sales),
            TotalItems = totalCount,
            CurrentPage = request.Page,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.Size)
        };
    }
}
