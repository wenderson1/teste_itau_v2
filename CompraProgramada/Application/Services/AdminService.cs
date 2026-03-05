using Application.Common;
using Application.DTOs.Responses;
using Application.Interfaces;
using Domain.Interfaces;

namespace Application.Services;

public class AdminService : IAdminService
{
    private readonly IContaGraficaRepository _contaGraficaRepository;
    private readonly ICustodiaRepository _custodiaRepository;
    private readonly ICotacaoService _cotacaoService;

    public AdminService(
        IContaGraficaRepository contaGraficaRepository,
        ICustodiaRepository custodiaRepository,
        ICotacaoService cotacaoService)
    {
        _contaGraficaRepository = contaGraficaRepository;
        _custodiaRepository = custodiaRepository;
        _cotacaoService = cotacaoService;
    }

    public async Task<Result<CustodiaMasterResponse>> ObterCustodiaMasterAsync()
    {
        var contaMaster = await _contaGraficaRepository.ObterContaMasterAsync();

        if (contaMaster == null)
        {
            return Error.NotFound("Conta Master nao encontrada.", "CONTA_MASTER_NAO_ENCONTRADA");
        }

        var custodias = await _custodiaRepository.ObterPorContaGraficaIdAsync(contaMaster.Id);
        var tickers = custodias.Select(c => c.Ticker);
        var cotacoes = await _cotacaoService.ObterCotacoesAsync(tickers);

        var custodiaItems = custodias.Select(c =>
        {
            var cotacaoAtual = cotacoes.GetValueOrDefault(c.Ticker, c.PrecoMedio);
            var valorAtual = c.Quantidade * cotacaoAtual;

            return new CustodiaItemResponse(
                Ticker: c.Ticker,
                Quantidade: c.Quantidade,
                PrecoMedio: Math.Round(c.PrecoMedio, 2),
                CotacaoAtual: Math.Round(cotacaoAtual, 2),
                ValorAtual: Math.Round(valorAtual, 2),
                Origem: "Residuo de distribuicao"
            );
        }).ToList();

        var valorTotalResiduo = custodiaItems.Sum(c => c.ValorAtual);

        return new CustodiaMasterResponse(
            ContaMaster: new ContaMasterResponse(
                Id: contaMaster.Id,
                NumeroConta: contaMaster.NumeroConta,
                Tipo: contaMaster.Tipo.ToString().ToUpper()
            ),
            Custodia: custodiaItems,
            ValorTotalResiduo: Math.Round(valorTotalResiduo, 2)
        );
    }
}
