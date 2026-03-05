namespace Application.DTOs.Responses;

public record DistribuicaoAporteResponse(
    string Ticker,
    int Quantidade,
    decimal PrecoUnitario,
    decimal Valor
);

public record AporteResponse(
    DateTime Data,
    decimal Valor,
    List<DistribuicaoAporteResponse> Distribuicao
);

public record EvolucaoCarteiraResponse(
    DateTime Data,
    decimal ValorInvestido,
    decimal ValorCarteira,
    decimal PL,
    decimal RentabilidadePercentual
);

public record RentabilidadeResponse(
    long ClienteId,
    string Nome,
    DateTime DataConsulta,
    ResumoCarteiraResponse Rentabilidade,
    List<AporteResponse> HistoricoAportes,
    List<EvolucaoCarteiraResponse> EvolucaoCarteira
);
