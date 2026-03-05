using Application.Common;
using Application.DTOs.Requests;
using Application.DTOs.Responses;

namespace Application.Interfaces;

public interface ICestaService
{
    Task<Result<CriarCestaResponse>> CriarCestaAsync(CriarCestaRequest request);
    Task<Result<CestaAtualResponse>> ObterCestaAtualAsync();
    Task<Result<CestaHistoricoResponse>> ObterHistoricoAsync();
}
