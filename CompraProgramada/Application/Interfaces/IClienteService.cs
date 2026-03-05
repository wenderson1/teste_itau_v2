using Application.Common;
using Application.DTOs.Requests;
using Application.DTOs.Responses;

namespace Application.Interfaces;

public interface IClienteService
{
    Task<Result<AdesaoResponse>> AderirAsync(AdesaoRequest request);
    Task<Result<SaidaResponse>> SairAsync(long clienteId);
    Task<Result<AlterarValorMensalResponse>> AlterarValorMensalAsync(long clienteId, AlterarValorMensalRequest request);
    Task<Result<CarteiraResponse>> ConsultarCarteiraAsync(long clienteId);
    Task<Result<RentabilidadeResponse>> ConsultarRentabilidadeAsync(long clienteId);
}
