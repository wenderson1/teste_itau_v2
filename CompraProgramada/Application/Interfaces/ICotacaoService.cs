namespace Application.Interfaces;

public interface ICotacaoService
{
    Task<decimal?> ObterCotacaoFechamentoAsync(string ticker);
    Task<Dictionary<string, decimal>> ObterCotacoesAsync(IEnumerable<string> tickers);
}
