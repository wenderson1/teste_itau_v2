namespace Application.DTOs.Requests;

public record ItemCestaRequest(
    string Ticker,
    decimal Percentual
);

public record CriarCestaRequest(
    string Nome,
    List<ItemCestaRequest> Itens
);
