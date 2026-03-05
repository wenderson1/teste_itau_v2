namespace Application.DTOs.Responses;

public record CustodiaItemResponse(
    string Ticker,
    int Quantidade,
    decimal PrecoMedio,
    decimal CotacaoAtual,
    decimal ValorAtual,
    string Origem
);

public record ContaMasterResponse(
    long Id,
    string NumeroConta,
    string Tipo
);

public record CustodiaMasterResponse(
    ContaMasterResponse ContaMaster,
    List<CustodiaItemResponse> Custodia,
    decimal ValorTotalResiduo
);
