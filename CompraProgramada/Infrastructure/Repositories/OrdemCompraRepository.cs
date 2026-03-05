using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class OrdemCompraRepository : IOrdemCompraRepository
{
    private readonly CompraProgramadaDbContext _context;

    public OrdemCompraRepository(CompraProgramadaDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<OrdemCompra>> ObterPorDataAsync(DateTime data)
    {
        var dataInicio = data.Date;
        var dataFim = data.Date.AddDays(1);

        return await _context.OrdensCompra
            .Where(o => o.DataOrdem >= dataInicio && o.DataOrdem < dataFim)
            .ToListAsync();
    }

    public async Task<bool> ExisteOrdemParaDataAsync(DateTime data)
    {
        var dataInicio = data.Date;
        var dataFim = data.Date.AddDays(1);

        return await _context.OrdensCompra
            .AnyAsync(o => o.DataOrdem >= dataInicio && o.DataOrdem < dataFim);
    }

    public async Task<OrdemCompra> CriarAsync(OrdemCompra ordem)
    {
        _context.OrdensCompra.Add(ordem);
        await _context.SaveChangesAsync();
        return ordem;
    }

    public async Task CriarVariasAsync(IEnumerable<OrdemCompra> ordens)
    {
        _context.OrdensCompra.AddRange(ordens);
        await _context.SaveChangesAsync();
    }

    public async Task AtualizarAsync(OrdemCompra ordem)
    {
        _context.OrdensCompra.Update(ordem);
        await _context.SaveChangesAsync();
    }
}
