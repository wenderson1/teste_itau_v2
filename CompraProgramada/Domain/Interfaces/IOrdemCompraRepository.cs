using Domain.Entities;

namespace Domain.Interfaces;

public interface IOrdemCompraRepository
{
    Task<IEnumerable<OrdemCompra>> ObterPorDataAsync(DateTime data);
    Task<bool> ExisteOrdemParaDataAsync(DateTime data);
    Task<OrdemCompra> CriarAsync(OrdemCompra ordem);
    Task CriarVariasAsync(IEnumerable<OrdemCompra> ordens);
    Task AtualizarAsync(OrdemCompra ordem);
}
