namespace Application.DTOs.Responses;

public record AtivoCarteiraResponse(
    string Ticker,
    int Quantidade,
    decimal PrecoMedio,
    decimal CotacaoAtual,
    decimal ValorAtual,
    decimal PL,
    decimal PLPercentual,
    decimal ComposicaoCarteira
);

public record ResumoCarteiraResponse(
    decimal ValorTotalInvestido,
    decimal ValorAtualCarteira,
    decimal PLTotal,
    decimal RentabilidadePercentual
);

public record CarteiraResponse(
    long ClienteId,
    string Nome,
    string ContaGrafica,
    DateTime DataConsulta,
    ResumoCarteiraResponse Resumo,
    List<AtivoCarteiraResponse> Ativos
);
