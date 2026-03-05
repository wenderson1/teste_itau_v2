using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ClienteRepository : IClienteRepository
{
    private readonly CompraProgramadaDbContext _context;

    public ClienteRepository(CompraProgramadaDbContext context)
    {
        _context = context;
    }

    public async Task<Cliente?> ObterPorIdAsync(long id)
    {
        return await _context.Clientes.FindAsync(id);
    }

    public async Task<Cliente?> ObterPorIdComContaGraficaAsync(long id)
    {
        return await _context.Clientes
            .Include(c => c.ContaGrafica)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Cliente?> ObterPorCpfAsync(string cpf)
    {
        return await _context.Clientes
            .FirstOrDefaultAsync(c => c.CPF == cpf);
    }

    public async Task<IEnumerable<Cliente>> ObterAtivosAsync()
    {
        return await _context.Clientes
            .Where(c => c.Ativo)
            .ToListAsync();
    }

    public async Task<Cliente> CriarAsync(Cliente cliente)
    {
        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();
        return cliente;
    }

    public async Task AtualizarAsync(Cliente cliente)
    {
        _context.Clientes.Update(cliente);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistePorCpfAsync(string cpf)
    {
        return await _context.Clientes.AnyAsync(c => c.CPF == cpf);
    }
}
