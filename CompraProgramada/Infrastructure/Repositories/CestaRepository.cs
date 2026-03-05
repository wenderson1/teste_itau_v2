using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class CestaRepository : ICestaRepository
{
    private readonly CompraProgramadaDbContext _context;

    public CestaRepository(CompraProgramadaDbContext context)
    {
        _context = context;
    }

    public async Task<Cesta?> ObterPorIdAsync(long id)
    {
        return await _context.Cestas.FindAsync(id);
    }

    public async Task<Cesta?> ObterPorIdComItensAsync(long id)
    {
        return await _context.Cestas
            .Include(c => c.Itens)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Cesta?> ObterAtivaAsync()
    {
        return await _context.Cestas
            .FirstOrDefaultAsync(c => c.Ativa);
    }

    public async Task<Cesta?> ObterAtivaComItensAsync()
    {
        return await _context.Cestas
            .Include(c => c.Itens)
            .FirstOrDefaultAsync(c => c.Ativa);
    }

    public async Task<IEnumerable<Cesta>> ObterTodasComItensAsync()
    {
        return await _context.Cestas
            .Include(c => c.Itens)
            .OrderByDescending(c => c.DataCriacao)
            .ToListAsync();
    }

    public async Task<Cesta> CriarAsync(Cesta cesta)
    {
        _context.Cestas.Add(cesta);
        await _context.SaveChangesAsync();
        return cesta;
    }

    public async Task AtualizarAsync(Cesta cesta)
    {
        _context.Cestas.Update(cesta);
        await _context.SaveChangesAsync();
    }
}
