using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace Ambev.DeveloperEvaluation.Infrastructure.Repositories;

public sealed class SaleRepository : ISaleRepository
{
    private readonly ApplicationDbContext _context;

    public SaleRepository(ApplicationDbContext context) => _context = context;

    public async Task<Sale> CreateAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        await _context.Sales.AddAsync(sale, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return sale;
    }

    public async Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task<Sale?> GetByNumberAsync(string saleNumber, CancellationToken cancellationToken = default) =>
        await _context.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.SaleNumber == saleNumber, cancellationToken);

    public async Task<(IEnumerable<Sale> Sales, int TotalCount)> GetPagedAsync(
        int page,
        int size,
        string? orderBy,
        Guid? customerId,
        Guid? branchId,
        string? status,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Sales.Include(s => s.Items).AsQueryable();

        if (customerId.HasValue)
            query = query.Where(s => s.CustomerId == customerId.Value);
        if (branchId.HasValue)
            query = query.Where(s => s.BranchId == branchId.Value);
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(s => s.Status.ToString() == status);

        var totalCount = await query.CountAsync(cancellationToken);

        query = !string.IsNullOrWhiteSpace(orderBy)
            ? query.OrderBy(orderBy)
            : query.OrderByDescending(s => s.SaleDate);

        var sales = await query
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(cancellationToken);

        return (sales, totalCount);
    }

    public async Task<Sale> UpdateAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        _context.Sales.Update(sale);
        await _context.SaveChangesAsync(cancellationToken);
        return sale;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var sale = await _context.Sales.FindAsync([id], cancellationToken);
        if (sale is null) return false;
        _context.Sales.Remove(sale);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
