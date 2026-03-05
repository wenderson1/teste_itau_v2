using Application.Common;
using Application.DTOs.Requests;
using Application.DTOs.Responses;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/motor")]
public class MotorController : ControllerBase
{
    private readonly ICompraService _compraService;

    public MotorController(ICompraService compraService)
    {
        _compraService = compraService;
    }

    /// <summary>
    /// Executar compra programada manualmente (para testes)
    /// </summary>
    [HttpPost("executar-compra")]
    [ProducesResponseType(typeof(ExecutarCompraResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ExecutarCompra([FromBody] ExecutarCompraRequest request)
    {
        var result = await _compraService.ExecutarCompraAsync(request);

        return result.Match<IActionResult>(
            onSuccess: response => Ok(response),
            onFailure: error => ToErrorResponse(error)
        );
    }

    /// <summary>
    /// Verificar próxima data de compra
    /// </summary>
    [HttpGet("proxima-data")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult ObterProximaDataCompra([FromQuery] DateOnly? dataBase = null)
    {
        var data = dataBase ?? DateOnly.FromDateTime(DateTime.Today);
        var proximaData = _compraService.ObterProximaDataCompra(data);

        return Ok(new
        {
            DataBase = data,
            ProximaDataCompra = proximaData,
            DiasRestantes = proximaData.DayNumber - data.DayNumber
        });
    }

    /// <summary>
    /// Verificar se uma data é data de compra
    /// </summary>
    [HttpGet("verificar-data")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult VerificarDataCompra([FromQuery] DateOnly data)
    {
        var ehDataCompra = _compraService.EhDataCompra(data);

        return Ok(new
        {
            Data = data,
            EhDataCompra = ehDataCompra,
            DiaDaSemana = data.DayOfWeek.ToString()
        });
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
