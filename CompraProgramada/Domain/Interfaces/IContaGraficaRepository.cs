using Domain.Entities;

namespace Domain.Interfaces;

public interface IContaGraficaRepository
{
    Task<ContaGrafica?> ObterPorIdAsync(long id);
    Task<ContaGrafica?> ObterPorClienteIdAsync(long clienteId);
    Task<ContaGrafica?> ObterContaMasterAsync();
    Task<ContaGrafica> CriarAsync(ContaGrafica contaGrafica);
    Task<string> GerarProximoNumeroContaFilhoteAsync();
}
