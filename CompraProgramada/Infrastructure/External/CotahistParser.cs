using System.Text;

namespace Infrastructure.External;

public class CotacaoB3
{
    public DateTime DataPregao { get; set; }
    public string Ticker { get; set; } = string.Empty;
    public string CodigoBDI { get; set; } = string.Empty;
    public int TipoMercado { get; set; }
    public string NomeEmpresa { get; set; } = string.Empty;
    public decimal PrecoAbertura { get; set; }
    public decimal PrecoMaximo { get; set; }
    public decimal PrecoMinimo { get; set; }
    public decimal PrecoFechamento { get; set; }
    public long QuantidadeNegociada { get; set; }
    public decimal VolumeNegociado { get; set; }
}

public class CotahistParser
{
    private const int TAMANHO_LINHA = 245;

    /// <summary>
    /// Faz o parse de um arquivo COTAHIST completo
    /// </summary>
    public IEnumerable<CotacaoB3> ParseArquivo(string caminhoArquivo)
    {
        if (!File.Exists(caminhoArquivo))
        {
            throw new FileNotFoundException($"Arquivo COTAHIST nao encontrado: {caminhoArquivo}");
        }

        // Encoding Latin1 (ISO-8859-1) usado pela B3
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var encoding = Encoding.GetEncoding("ISO-8859-1");

        foreach (var linha in File.ReadLines(caminhoArquivo, encoding))
        {
            // Ignora linhas menores que o esperado ou headers/trailers
            if (linha.Length < TAMANHO_LINHA)
                continue;

            var tipoRegistro = linha.Substring(0, 2);

            // Processa apenas registros de detalhe (tipo 01)
            if (tipoRegistro != "01")
                continue;

            var cotacao = ParseLinha(linha);
            if (cotacao != null)
            {
                yield return cotacao;
            }
        }
    }

    /// <summary>
    /// Faz o parse de uma linha individual do COTAHIST
    /// </summary>
    public CotacaoB3? ParseLinha(string linha)
    {
        if (string.IsNullOrEmpty(linha) || linha.Length < TAMANHO_LINHA)
            return null;

        try
        {
            // Posições conforme layout B3 (1-indexed no doc, 0-indexed aqui)
            var dataPregao = ParseData(linha.Substring(2, 8));
            var codigoBdi = linha.Substring(10, 2).Trim();
            var ticker = linha.Substring(12, 12).Trim();
            var tipoMercado = int.Parse(linha.Substring(24, 3));
            var nomeEmpresa = linha.Substring(27, 12).Trim();

            // Preços (dividir por 100 - 2 casas decimais implícitas)
            var precoAbertura = ParsePreco(linha.Substring(56, 13));
            var precoMaximo = ParsePreco(linha.Substring(69, 13));
            var precoMinimo = ParsePreco(linha.Substring(82, 13));
            var precoFechamento = ParsePreco(linha.Substring(108, 13));

            var quantidadeNegociada = long.Parse(linha.Substring(152, 18));
            var volumeNegociado = ParsePreco(linha.Substring(170, 18));

            return new CotacaoB3
            {
                DataPregao = dataPregao,
                Ticker = ticker,
                CodigoBDI = codigoBdi,
                TipoMercado = tipoMercado,
                NomeEmpresa = nomeEmpresa,
                PrecoAbertura = precoAbertura,
                PrecoMaximo = precoMaximo,
                PrecoMinimo = precoMinimo,
                PrecoFechamento = precoFechamento,
                QuantidadeNegociada = quantidadeNegociada,
                VolumeNegociado = volumeNegociado
            };
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Obtém cotação de fechamento de um ticker específico
    /// </summary>
    public CotacaoB3? ObterCotacaoFechamento(string pastaArquivos, string ticker)
    {
        // Busca o arquivo mais recente na pasta
        var arquivo = ObterArquivoMaisRecente(pastaArquivos);
        if (arquivo == null)
            return null;

        // Filtra por BDI 02 (lote padrão) ou 96 (fracionário)
        // e tipo mercado 010 (vista) ou 020 (fracionário)
        return ParseArquivo(arquivo)
            .Where(c => c.Ticker.Equals(ticker, StringComparison.OrdinalIgnoreCase))
            .Where(c => c.CodigoBDI == "02" || c.CodigoBDI == "96")
            .Where(c => c.TipoMercado == 10 || c.TipoMercado == 20)
            .OrderByDescending(c => c.DataPregao)
            .FirstOrDefault();
    }

    /// <summary>
    /// Obtém cotações de fechamento para múltiplos tickers
    /// </summary>
    public Dictionary<string, decimal> ObterCotacoes(string pastaArquivos, IEnumerable<string> tickers)
    {
        var arquivo = ObterArquivoMaisRecente(pastaArquivos);
        if (arquivo == null)
            return new Dictionary<string, decimal>();

        var tickersUpper = tickers.Select(t => t.ToUpper()).ToHashSet();

        var cotacoes = ParseArquivo(arquivo)
            .Where(c => tickersUpper.Contains(c.Ticker.ToUpper()))
            .Where(c => c.CodigoBDI == "02") // Apenas lote padrão
            .Where(c => c.TipoMercado == 10) // Mercado à vista
            .GroupBy(c => c.Ticker.ToUpper())
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(c => c.DataPregao).First().PrecoFechamento
            );

        return cotacoes;
    }

    private string? ObterArquivoMaisRecente(string pastaArquivos)
    {
        if (!Directory.Exists(pastaArquivos))
            return null;

        return Directory.GetFiles(pastaArquivos, "COTAHIST_D*.TXT")
            .OrderByDescending(f => f)
            .FirstOrDefault();
    }

    private static DateTime ParseData(string valor)
    {
        // Formato AAAAMMDD
        var ano = int.Parse(valor.Substring(0, 4));
        var mes = int.Parse(valor.Substring(4, 2));
        var dia = int.Parse(valor.Substring(6, 2));
        return new DateTime(ano, mes, dia);
    }

    private static decimal ParsePreco(string valor)
    {
        // Preços com 2 casas decimais implícitas
        if (long.TryParse(valor, out var valorInteiro))
        {
            return valorInteiro / 100m;
        }
        return 0m;
    }
}
