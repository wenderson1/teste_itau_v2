using Application.Interfaces;

namespace Application.Services;

/// <summary>
/// Implementação stub do serviço de cotação.
/// Em produção, deve ler do arquivo COTAHIST da B3.
/// </summary>
public class CotacaoService : ICotacaoService
{
    // Cotações simuladas para testes
    private readonly Dictionary<string, decimal> _cotacoesSimuladas = new()
    {
        { "PETR4", 37.00m },
        { "VALE3", 65.00m },
        { "ITUB4", 31.00m },
        { "BBDC4", 15.50m },
        { "WEGE3", 42.00m }
    };

    public Task<decimal?> ObterCotacaoFechamentoAsync(string ticker)
    {
        if (_cotacoesSimuladas.TryGetValue(ticker.ToUpper(), out var cotacao))
        {
            return Task.FromResult<decimal?>(cotacao);
        }
        return Task.FromResult<decimal?>(null);
    }

    public Task<Dictionary<string, decimal>> ObterCotacoesAsync(IEnumerable<string> tickers)
    {
        var resultado = new Dictionary<string, decimal>();
        foreach (var ticker in tickers)
        {
            if (_cotacoesSimuladas.TryGetValue(ticker.ToUpper(), out var cotacao))
            {
                resultado[ticker] = cotacao;
            }
        }
        return Task.FromResult(resultado);
    }
}
