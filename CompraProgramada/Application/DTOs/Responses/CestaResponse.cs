namespace Application.DTOs.Responses;

public record ItemCestaResponse(
    string Ticker,
    decimal Percentual
);

public record ItemCestaComCotacaoResponse(
    string Ticker,
    decimal Percentual,
    decimal CotacaoAtual
);

public record CestaAnteriorDesativadaResponse(
    long CestaId,
    string Nome,
    DateTime DataDesativacao
);

public record CriarCestaResponse(
    long CestaId,
    string Nome,
    bool Ativa,
    DateTime DataCriacao,
    List<ItemCestaResponse> Itens,
    bool RebalanceamentoDisparado,
    CestaAnteriorDesativadaResponse? CestaAnteriorDesativada,
    List<string>? AtivosRemovidos,
    List<string>? AtivosAdicionados,
    string Mensagem
);

public record CestaAtualResponse(
    long CestaId,
    string Nome,
    bool Ativa,
    DateTime DataCriacao,
    List<ItemCestaComCotacaoResponse> Itens
);

public record CestaHistoricoItemResponse(
    long CestaId,
    string Nome,
    bool Ativa,
    DateTime DataCriacao,
    DateTime? DataDesativacao,
    List<ItemCestaResponse> Itens
);

public record CestaHistoricoResponse(
    List<CestaHistoricoItemResponse> Cestas
);
