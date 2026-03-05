namespace Application.DTOs.Responses;

public record ContaGraficaResponse(
    long Id,
    string NumeroConta,
    string Tipo,
    DateTime DataCriacao
);

public record AdesaoResponse(
    long ClienteId,
    string Nome,
    string CPF,
    string Email,
    decimal ValorMensal,
    bool Ativo,
    DateTime DataAdesao,
    ContaGraficaResponse ContaGrafica
);
