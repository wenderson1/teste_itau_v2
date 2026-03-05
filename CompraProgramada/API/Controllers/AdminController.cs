using Application.Common;
using Application.DTOs.Requests;
using Application.DTOs.Responses;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly ICestaService _cestaService;
    private readonly IAdminService _adminService;

    public AdminController(ICestaService cestaService, IAdminService adminService)
    {
        _cestaService = cestaService;
        _adminService = adminService;
    }

    /// <summary>
    /// Cadastrar / Alterar Cesta Top Five
    /// </summary>
    [HttpPost("cesta")]
    [ProducesResponseType(typeof(CriarCestaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CriarCesta([FromBody] CriarCestaRequest request)
    {
        var result = await _cestaService.CriarCestaAsync(request);

        return result.Match<IActionResult>(
            onSuccess: response => CreatedAtAction(nameof(ObterCestaAtual), response),
            onFailure: error => ToErrorResponse(error)
        );
    }

    /// <summary>
    /// Consultar Cesta Atual (Ativa)
    /// </summary>
    [HttpGet("cesta/atual")]
    [ProducesResponseType(typeof(CestaAtualResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterCestaAtual()
    {
        var result = await _cestaService.ObterCestaAtualAsync();

        return result.Match<IActionResult>(
            onSuccess: response => Ok(response),
            onFailure: error => ToErrorResponse(error)
        );
    }

    /// <summary>
    /// Histórico de Cestas
    /// </summary>
    [HttpGet("cesta/historico")]
    [ProducesResponseType(typeof(CestaHistoricoResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterHistoricoCestas()
    {
        var result = await _cestaService.ObterHistoricoAsync();

        return result.Match<IActionResult>(
            onSuccess: response => Ok(response),
            onFailure: error => ToErrorResponse(error)
        );
    }

    /// <summary>
    /// Consultar Custódia da Conta Master
    /// </summary>
    [HttpGet("conta-master/custodia")]
    [ProducesResponseType(typeof(CustodiaMasterResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterCustodiaMaster()
    {
        var result = await _adminService.ObterCustodiaMasterAsync();

        return result.Match<IActionResult>(
            onSuccess: response => Ok(response),
            onFailure: error => ToErrorResponse(error)
        );
    }

    private IActionResult ToErrorResponse(Error error)
    {
        var erroResponse = new ErroResponse(error.Mensagem, error.Codigo);

        return error.Tipo switch
        {
            ErrorType.NotFound => NotFound(erroResponse),
            ErrorType.Validation => BadRequest(erroResponse),
            ErrorType.Conflict => Conflict(erroResponse),
            _ => StatusCode(StatusCodes.Status500InternalServerError, erroResponse)
        };
    }
}
