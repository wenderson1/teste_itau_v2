using Application.Interfaces;
using Infrastructure.External;

namespace Application.Services;

public class CotacaoServiceOptions
{
    public string CotacoesPath { get; set; } = "cotacoes";
}

/// <summary>
/// Serviço de cotação que lê do arquivo COTAHIST da B3.
/// Fallback para cotações simuladas quando arquivo não disponível.
/// </summary>
public class CotacaoService : ICotacaoService
{
    private readonly CotahistParser _parser;
    private readonly string _pastaArquivos;

    // Cotações simuladas para fallback/testes
    private readonly Dictionary<string, decimal> _cotacoesSimuladas = new()
    {
        { "PETR4", 37.00m },
        { "VALE3", 65.00m },
        { "ITUB4", 31.00m },
        { "BBDC4", 15.50m },
        { "WEGE3", 42.00m },
        { "ABEV3", 14.50m },
        { "RENT3", 49.00m }
    };

    public CotacaoService(CotacaoServiceOptions? options = null)
    {
        _parser = new CotahistParser();
        _pastaArquivos = options?.CotacoesPath ?? "cotacoes";
    }

    public Task<decimal?> ObterCotacaoFechamentoAsync(string ticker)
    {
        try
        {
            var cotacao = _parser.ObterCotacaoFechamento(_pastaArquivos, ticker);
            if (cotacao != null)
            {
                return Task.FromResult<decimal?>(cotacao.PrecoFechamento);
            }
        }
        catch
        {
            // Fallback para cotações simuladas
        }

        // Fallback
        if (_cotacoesSimuladas.TryGetValue(ticker.ToUpper(), out var cotacaoSimulada))
        {
            return Task.FromResult<decimal?>(cotacaoSimulada);
        }
        return Task.FromResult<decimal?>(null);
    }

    public Task<Dictionary<string, decimal>> ObterCotacoesAsync(IEnumerable<string> tickers)
    {
        try
        {
            var cotacoes = _parser.ObterCotacoes(_pastaArquivos, tickers);
            if (cotacoes.Count > 0)
            {
                return Task.FromResult(cotacoes);
            }
        }
        catch
        {
            // Fallback para cotações simuladas
        }

        // Fallback
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
