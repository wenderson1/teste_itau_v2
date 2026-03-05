using Domain.Entities;

namespace Domain.Interfaces;

public interface ICestaRepository
{
    Task<Cesta?> ObterPorIdAsync(long id);
    Task<Cesta?> ObterPorIdComItensAsync(long id);
    Task<Cesta?> ObterAtivaAsync();
    Task<Cesta?> ObterAtivaComItensAsync();
    Task<IEnumerable<Cesta>> ObterTodasComItensAsync();
    Task<Cesta> CriarAsync(Cesta cesta);
    Task AtualizarAsync(Cesta cesta);
}
