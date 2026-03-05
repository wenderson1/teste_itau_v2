using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class HistoricoAporteRepository : IHistoricoAporteRepository
{
    private readonly CompraProgramadaDbContext _context;

    public HistoricoAporteRepository(CompraProgramadaDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<HistoricoAporte>> ObterPorClienteIdAsync(long clienteId)
    {
        return await _context.HistoricoAportes
            .Include(h => h.Distribuicoes)
            .Where(h => h.ClienteId == clienteId)
            .OrderByDescending(h => h.DataAporte)
            .ToListAsync();
    }

    public async Task<HistoricoAporte> CriarAsync(HistoricoAporte historicoAporte)
    {
        _context.HistoricoAportes.Add(historicoAporte);
        await _context.SaveChangesAsync();
        return historicoAporte;
    }
}
