using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ContaGraficaRepository : IContaGraficaRepository
{
    private readonly CompraProgramadaDbContext _context;

    public ContaGraficaRepository(CompraProgramadaDbContext context)
    {
        _context = context;
    }

    public async Task<ContaGrafica?> ObterPorIdAsync(long id)
    {
        return await _context.ContasGraficas.FindAsync(id);
    }

    public async Task<ContaGrafica?> ObterPorClienteIdAsync(long clienteId)
    {
        return await _context.ContasGraficas
            .FirstOrDefaultAsync(c => c.ClienteId == clienteId);
    }

    public async Task<ContaGrafica?> ObterContaMasterAsync()
    {
        return await _context.ContasGraficas
            .FirstOrDefaultAsync(c => c.Tipo == TipoConta.Master);
    }

    public async Task<ContaGrafica> CriarAsync(ContaGrafica contaGrafica)
    {
        _context.ContasGraficas.Add(contaGrafica);
        await _context.SaveChangesAsync();
        return contaGrafica;
    }

    public async Task<string> GerarProximoNumeroContaFilhoteAsync()
    {
        var ultimaConta = await _context.ContasGraficas
            .Where(c => c.Tipo == TipoConta.Filhote)
            .OrderByDescending(c => c.Id)
            .FirstOrDefaultAsync();

        int proximoNumero = 1;
        if (ultimaConta != null && ultimaConta.NumeroConta.StartsWith("FLH-"))
        {
            var numeroAtual = ultimaConta.NumeroConta.Replace("FLH-", "");
            if (int.TryParse(numeroAtual, out int numero))
            {
                proximoNumero = numero + 1;
            }
        }

        return $"FLH-{proximoNumero:D6}";
    }
}
