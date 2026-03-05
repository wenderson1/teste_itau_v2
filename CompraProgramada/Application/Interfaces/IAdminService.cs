using Application.Common;
using Application.DTOs.Responses;

namespace Application.Interfaces;

public interface IAdminService
{
    Task<Result<CustodiaMasterResponse>> ObterCustodiaMasterAsync();
}
