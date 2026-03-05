using Domain.Entities;

namespace Domain.Interfaces;

public interface IHistoricoAporteRepository
{
    Task<IEnumerable<HistoricoAporte>> ObterPorClienteIdAsync(long clienteId);
    Task<HistoricoAporte> CriarAsync(HistoricoAporte historicoAporte);
}
