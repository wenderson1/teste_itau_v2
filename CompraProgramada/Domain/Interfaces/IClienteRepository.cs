using Domain.Entities;

namespace Domain.Interfaces;

public interface IClienteRepository
{
    Task<Cliente?> ObterPorIdAsync(long id);
    Task<Cliente?> ObterPorIdComContaGraficaAsync(long id);
    Task<Cliente?> ObterPorCpfAsync(string cpf);
    Task<IEnumerable<Cliente>> ObterAtivosAsync();
    Task<Cliente> CriarAsync(Cliente cliente);
    Task AtualizarAsync(Cliente cliente);
    Task<bool> ExistePorCpfAsync(string cpf);
}
