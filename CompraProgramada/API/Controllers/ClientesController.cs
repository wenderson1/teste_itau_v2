using Application.Common;
using Application.DTOs.Requests;
using Application.DTOs.Responses;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/clientes")]
public class ClientesController : ControllerBase
{
    private readonly IClienteService _clienteService;

    public ClientesController(IClienteService clienteService)
    {
        _clienteService = clienteService;
    }

    /// <summary>
    /// Aderir ao produto de Compra Programada
    /// </summary>
    [HttpPost("adesao")]
    [ProducesResponseType(typeof(AdesaoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Aderir([FromBody] AdesaoRequest request)
    {
        var result = await _clienteService.AderirAsync(request);

        return result.Match<IActionResult>(
            onSuccess: response => CreatedAtAction(nameof(ConsultarCarteira), new { clienteId = response.ClienteId }, response),
            onFailure: error => ToErrorResponse(error)
        );
    }

    /// <summary>
    /// Sair do produto de Compra Programada
    /// </summary>
    [HttpPost("{clienteId}/saida")]
    [ProducesResponseType(typeof(SaidaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Sair(long clienteId)
    {
        var result = await _clienteService.SairAsync(clienteId);

        return result.Match<IActionResult>(
            onSuccess: response => Ok(response),
            onFailure: error => ToErrorResponse(error)
        );
    }

    /// <summary>
    /// Alterar valor mensal do aporte
    /// </summary>
    [HttpPut("{clienteId}/valor-mensal")]
    [ProducesResponseType(typeof(AlterarValorMensalResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AlterarValorMensal(long clienteId, [FromBody] AlterarValorMensalRequest request)
    {
        var result = await _clienteService.AlterarValorMensalAsync(clienteId, request);

        return result.Match<IActionResult>(
            onSuccess: response => Ok(response),
            onFailure: error => ToErrorResponse(error)
        );
    }

    /// <summary>
    /// Consultar carteira (custódia) do cliente
    /// </summary>
    [HttpGet("{clienteId}/carteira")]
    [ProducesResponseType(typeof(CarteiraResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConsultarCarteira(long clienteId)
    {
        var result = await _clienteService.ConsultarCarteiraAsync(clienteId);

        return result.Match<IActionResult>(
            onSuccess: response => Ok(response),
            onFailure: error => ToErrorResponse(error)
        );
    }

    /// <summary>
    /// Consultar rentabilidade detalhada do cliente
    /// </summary>
    [HttpGet("{clienteId}/rentabilidade")]
    [ProducesResponseType(typeof(RentabilidadeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConsultarRentabilidade(long clienteId)
    {
        var result = await _clienteService.ConsultarRentabilidadeAsync(clienteId);

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
