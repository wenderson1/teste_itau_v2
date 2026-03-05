namespace Application.DTOs.Requests;

public record AdesaoRequest(
    string Nome,
    string CPF,
    string Email,
    decimal ValorMensal
);
