using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class CustodiaRepository : ICustodiaRepository
{
    private readonly CompraProgramadaDbContext _context;

    public CustodiaRepository(CompraProgramadaDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Custodia>> ObterPorContaGraficaIdAsync(long contaGraficaId)
    {
        return await _context.Custodias
            .Where(c => c.ContaGraficaId == contaGraficaId)
            .ToListAsync();
    }

    public async Task<Custodia?> ObterPorContaETickerAsync(long contaGraficaId, string ticker)
    {
        return await _context.Custodias
            .FirstOrDefaultAsync(c => c.ContaGraficaId == contaGraficaId && c.Ticker == ticker);
    }

    public async Task<Custodia> CriarAsync(Custodia custodia)
    {
        _context.Custodias.Add(custodia);
        await _context.SaveChangesAsync();
        return custodia;
    }

    public async Task AtualizarAsync(Custodia custodia)
    {
        _context.Custodias.Update(custodia);
        await _context.SaveChangesAsync();
    }
}
