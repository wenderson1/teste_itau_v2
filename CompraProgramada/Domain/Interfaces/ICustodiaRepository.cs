using Domain.Entities;

namespace Domain.Interfaces;

public interface ICustodiaRepository
{
    Task<IEnumerable<Custodia>> ObterPorContaGraficaIdAsync(long contaGraficaId);
    Task<Custodia?> ObterPorContaETickerAsync(long contaGraficaId, string ticker);
    Task<Custodia> CriarAsync(Custodia custodia);
    Task AtualizarAsync(Custodia custodia);
}
