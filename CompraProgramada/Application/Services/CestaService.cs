using Application.Common;
using Application.DTOs.Requests;
using Application.DTOs.Responses;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services;

public class CestaService : ICestaService
{
    private readonly ICestaRepository _cestaRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly ICotacaoService _cotacaoService;

    private const int QUANTIDADE_ATIVOS_CESTA = 5;
    private const decimal SOMA_PERCENTUAIS = 100m;

    public CestaService(
        ICestaRepository cestaRepository,
        IClienteRepository clienteRepository,
        ICotacaoService cotacaoService)
    {
        _cestaRepository = cestaRepository;
        _clienteRepository = clienteRepository;
        _cotacaoService = cotacaoService;
    }

    public async Task<Result<CriarCestaResponse>> CriarCestaAsync(CriarCestaRequest request)
    {
        // Validação: quantidade de ativos
        if (request.Itens.Count != QUANTIDADE_ATIVOS_CESTA)
        {
            return Error.Validation(
                $"A cesta deve conter exatamente {QUANTIDADE_ATIVOS_CESTA} ativos. Quantidade informada: {request.Itens.Count}.",
                "QUANTIDADE_ATIVOS_INVALIDA");
        }

        // Validação: percentuais maiores que zero
        if (request.Itens.Any(i => i.Percentual <= 0))
        {
            return Error.Validation(
                "Todos os percentuais devem ser maiores que 0%.",
                "PERCENTUAL_INVALIDO");
        }

        // Validação: soma dos percentuais
        var somaPercentuais = request.Itens.Sum(i => i.Percentual);
        if (somaPercentuais != SOMA_PERCENTUAIS)
        {
            return Error.Validation(
                $"A soma dos percentuais deve ser exatamente 100%. Soma atual: {somaPercentuais}%.",
                "PERCENTUAIS_INVALIDOS");
        }

        // Verificar cesta anterior ativa
        var cestaAnterior = await _cestaRepository.ObterAtivaComItensAsync();
        CestaAnteriorDesativadaResponse? cestaAnteriorDesativada = null;
        List<string>? ativosRemovidos = null;
        List<string>? ativosAdicionados = null;
        bool rebalanceamentoDisparado = false;
        string mensagem;

        if (cestaAnterior != null)
        {
            // Desativar cesta anterior
            cestaAnterior.Ativa = false;
            cestaAnterior.DataDesativacao = DateTime.UtcNow;
            await _cestaRepository.AtualizarAsync(cestaAnterior);

            cestaAnteriorDesativada = new CestaAnteriorDesativadaResponse(
                CestaId: cestaAnterior.Id,
                Nome: cestaAnterior.Nome,
                DataDesativacao: cestaAnterior.DataDesativacao.Value
            );

            // Identificar mudanças de ativos
            var tickersAnteriores = cestaAnterior.Itens.Select(i => i.Ticker).ToHashSet();
            var tickersNovos = request.Itens.Select(i => i.Ticker).ToHashSet();

            ativosRemovidos = tickersAnteriores.Except(tickersNovos).ToList();
            ativosAdicionados = tickersNovos.Except(tickersAnteriores).ToList();

            // Verificar se há rebalanceamento necessário
            if (ativosRemovidos.Count > 0 || ativosAdicionados.Count > 0)
            {
                rebalanceamentoDisparado = true;
                var clientesAtivos = await _clienteRepository.ObterAtivosAsync();
                mensagem = $"Cesta atualizada. Rebalanceamento disparado para {clientesAtivos.Count()} clientes ativos.";
            }
            else
            {
                mensagem = "Cesta atualizada com sucesso. Percentuais alterados.";
            }
        }
        else
        {
            mensagem = "Primeira cesta cadastrada com sucesso.";
        }

        // Criar nova cesta
        var dataCriacao = DateTime.UtcNow;
        var novaCesta = new Cesta
        {
            Nome = request.Nome,
            Ativa = true,
            DataCriacao = dataCriacao,
            Itens = request.Itens.Select(i => new ItemCesta
            {
                Ticker = i.Ticker.ToUpper(),
                Percentual = i.Percentual
            }).ToList()
        };

        await _cestaRepository.CriarAsync(novaCesta);

        var itensResponse = novaCesta.Itens.Select(i => new ItemCestaResponse(
            Ticker: i.Ticker,
            Percentual: i.Percentual
        )).ToList();

        return new CriarCestaResponse(
            CestaId: novaCesta.Id,
            Nome: novaCesta.Nome,
            Ativa: novaCesta.Ativa,
            DataCriacao: novaCesta.DataCriacao,
            Itens: itensResponse,
            RebalanceamentoDisparado: rebalanceamentoDisparado,
            CestaAnteriorDesativada: cestaAnteriorDesativada,
            AtivosRemovidos: ativosRemovidos,
            AtivosAdicionados: ativosAdicionados,
            Mensagem: mensagem
        );
    }

    public async Task<Result<CestaAtualResponse>> ObterCestaAtualAsync()
    {
        var cesta = await _cestaRepository.ObterAtivaComItensAsync();

        if (cesta == null)
        {
            return Error.NotFound("Nenhuma cesta ativa encontrada.", "CESTA_NAO_ENCONTRADA");
        }

        var tickers = cesta.Itens.Select(i => i.Ticker);
        var cotacoes = await _cotacaoService.ObterCotacoesAsync(tickers);

        var itens = cesta.Itens.Select(i => new ItemCestaComCotacaoResponse(
            Ticker: i.Ticker,
            Percentual: i.Percentual,
            CotacaoAtual: cotacoes.GetValueOrDefault(i.Ticker, 0m)
        )).ToList();

        return new CestaAtualResponse(
            CestaId: cesta.Id,
            Nome: cesta.Nome,
            Ativa: cesta.Ativa,
            DataCriacao: cesta.DataCriacao,
            Itens: itens
        );
    }

    public async Task<Result<CestaHistoricoResponse>> ObterHistoricoAsync()
    {
        var cestas = await _cestaRepository.ObterTodasComItensAsync();

        var cestasResponse = cestas.Select(c => new CestaHistoricoItemResponse(
            CestaId: c.Id,
            Nome: c.Nome,
            Ativa: c.Ativa,
            DataCriacao: c.DataCriacao,
            DataDesativacao: c.DataDesativacao,
            Itens: c.Itens.Select(i => new ItemCestaResponse(
                Ticker: i.Ticker,
                Percentual: i.Percentual
            )).ToList()
        )).ToList();

        return new CestaHistoricoResponse(Cestas: cestasResponse);
    }
}
