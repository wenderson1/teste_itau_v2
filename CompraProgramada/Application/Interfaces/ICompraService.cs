using Application.Common;
using Application.DTOs.Requests;
using Application.DTOs.Responses;

namespace Application.Interfaces;

public interface ICompraService
{
    /// <summary>
    /// Verifica se a data é uma data de compra programada (5, 15 ou 25 ajustado para dia útil)
    /// </summary>
    bool EhDataCompra(DateOnly data);

    /// <summary>
    /// Obtém a próxima data de compra a partir de uma data base
    /// </summary>
    DateOnly ObterProximaDataCompra(DateOnly dataBase);

    /// <summary>
    /// Executa a compra programada para uma data específica
    /// </summary>
    Task<Result<ExecutarCompraResponse>> ExecutarCompraAsync(ExecutarCompraRequest request);
}
