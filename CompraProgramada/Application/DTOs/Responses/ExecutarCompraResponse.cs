namespace Application.DTOs.Responses;

public record OrdemCompraResponse(
    string Ticker,
    string TipoOrdem,
    int Quantidade,
    decimal PrecoUnitario,
    decimal ValorTotal
);

public record DistribuicaoClienteResponse(
    long ClienteId,
    string Nome,
    string ContaGrafica,
    decimal ValorAporte,
    List<DistribuicaoAtivoResponse> Ativos
);

public record DistribuicaoAtivoResponse(
    string Ticker,
    int Quantidade,
    decimal PrecoUnitario,
    decimal Valor
);

public record ResiduoResponse(
    string Ticker,
    int Quantidade
);

public record ExecutarCompraResponse(
    DateTime DataExecucao,
    int TotalClientes,
    decimal TotalConsolidado,
    List<OrdemCompraResponse> OrdensCompra,
    List<DistribuicaoClienteResponse> Distribuicoes,
    List<ResiduoResponse> ResiduosCustMaster,
    int EventosIRPublicados,
    string Mensagem
);
