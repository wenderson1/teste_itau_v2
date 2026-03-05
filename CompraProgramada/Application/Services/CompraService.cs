using Application.Common;
using Application.DTOs.Requests;
using Application.DTOs.Responses;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services;

public class CompraService : ICompraService
{
    private readonly IClienteRepository _clienteRepository;
    private readonly IContaGraficaRepository _contaGraficaRepository;
    private readonly ICustodiaRepository _custodiaRepository;
    private readonly ICestaRepository _cestaRepository;
    private readonly IOrdemCompraRepository _ordemCompraRepository;
    private readonly IHistoricoAporteRepository _historicoAporteRepository;
    private readonly ICotacaoService _cotacaoService;

    private static readonly int[] DiasCompra = { 5, 15, 25 };

    public CompraService(
        IClienteRepository clienteRepository,
        IContaGraficaRepository contaGraficaRepository,
        ICustodiaRepository custodiaRepository,
        ICestaRepository cestaRepository,
        IOrdemCompraRepository ordemCompraRepository,
        IHistoricoAporteRepository historicoAporteRepository,
        ICotacaoService cotacaoService)
    {
        _clienteRepository = clienteRepository;
        _contaGraficaRepository = contaGraficaRepository;
        _custodiaRepository = custodiaRepository;
        _cestaRepository = cestaRepository;
        _ordemCompraRepository = ordemCompraRepository;
        _historicoAporteRepository = historicoAporteRepository;
        _cotacaoService = cotacaoService;
    }

    public bool EhDataCompra(DateOnly data)
    {
        // Ajusta para dia útil se cair em fim de semana
        var dataAjustada = AjustarParaDiaUtil(data);
        return DiasCompra.Any(d => AjustarParaDiaUtil(new DateOnly(data.Year, data.Month, d)) == dataAjustada);
    }

    public DateOnly ObterProximaDataCompra(DateOnly dataBase)
    {
        var ano = dataBase.Year;
        var mes = dataBase.Month;

        foreach (var dia in DiasCompra)
        {
            var dataCompra = AjustarParaDiaUtil(new DateOnly(ano, mes, dia));
            if (dataCompra >= dataBase)
                return dataCompra;
        }

        // Próximo mês
        var proximoMes = mes == 12 ? 1 : mes + 1;
        var proximoAno = mes == 12 ? ano + 1 : ano;
        return AjustarParaDiaUtil(new DateOnly(proximoAno, proximoMes, DiasCompra[0]));
    }

    public async Task<Result<ExecutarCompraResponse>> ExecutarCompraAsync(ExecutarCompraRequest request)
    {
        var dataReferencia = request.DataReferencia.ToDateTime(TimeOnly.MinValue);

        // Verificar se já existe ordem para esta data
        if (await _ordemCompraRepository.ExisteOrdemParaDataAsync(dataReferencia))
        {
            return Error.Validation("Compra ja foi executada para esta data.", "COMPRA_JA_EXECUTADA");
        }

        // Buscar cesta ativa
        var cesta = await _cestaRepository.ObterAtivaComItensAsync();
        if (cesta == null)
        {
            return Error.NotFound("Nenhuma cesta ativa encontrada.", "CESTA_NAO_ENCONTRADA");
        }

        // Buscar conta master
        var contaMaster = await _contaGraficaRepository.ObterContaMasterAsync();
        if (contaMaster == null)
        {
            return Error.NotFound("Conta Master nao encontrada.", "CONTA_MASTER_NAO_ENCONTRADA");
        }

        // Buscar clientes ativos
        var clientesAtivos = (await _clienteRepository.ObterAtivosAsync()).ToList();
        if (!clientesAtivos.Any())
        {
            return Error.Validation("Nenhum cliente ativo para processar.", "SEM_CLIENTES_ATIVOS");
        }

        // Obter cotações
        var tickers = cesta.Itens.Select(i => i.Ticker).ToList();
        var cotacoes = await _cotacaoService.ObterCotacoesAsync(tickers);

        if (cotacoes.Count < tickers.Count)
        {
            var tickersFaltando = tickers.Except(cotacoes.Keys).ToList();
            return Error.NotFound(
                $"Cotacao nao encontrada para: {string.Join(", ", tickersFaltando)}",
                "COTACAO_NAO_ENCONTRADA");
        }

        // Calcular 1/3 do valor mensal de cada cliente (3 datas por mês)
        var clientesComAportes = clientesAtivos.Select(c => new
        {
            Cliente = c,
            AporteData = c.ValorMensal / 3m
        }).ToList();

        var totalConsolidado = clientesComAportes.Sum(c => c.AporteData);

        // Calcular valor por ativo (consolidado)
        var valorPorAtivo = cesta.Itens.ToDictionary(
            i => i.Ticker,
            i => totalConsolidado * (i.Percentual / 100m)
        );

        // Obter saldo atual da custódia master
        var custodiaMaster = (await _custodiaRepository.ObterPorContaGraficaIdAsync(contaMaster.Id))
            .ToDictionary(c => c.Ticker, c => c);

        // Calcular quantidade a comprar por ticker
        var quantidadePorTicker = new Dictionary<string, int>();
        foreach (var ticker in tickers)
        {
            var valor = valorPorAtivo[ticker];
            var cotacao = cotacoes[ticker];
            var quantidadeCalculada = (int)Math.Truncate(valor / cotacao);

            // Descontar saldo master existente
            var saldoMaster = custodiaMaster.ContainsKey(ticker) ? custodiaMaster[ticker].Quantidade : 0;
            var quantidadeAComprar = Math.Max(0, quantidadeCalculada - saldoMaster);

            quantidadePorTicker[ticker] = quantidadeAComprar;
        }

        // Criar ordens de compra (separar lote padrão e fracionário)
        var ordensCompra = new List<OrdemCompra>();
        var ordensResponse = new List<OrdemCompraResponse>();

        foreach (var (ticker, quantidade) in quantidadePorTicker)
        {
            if (quantidade == 0) continue;

            var cotacao = cotacoes[ticker];
            var lotePadrao = quantidade / 100;
            var fracionario = quantidade % 100;

            // Lote padrão (múltiplos de 100)
            if (lotePadrao > 0)
            {
                var qtdLote = lotePadrao * 100;
                var ordem = new OrdemCompra
                {
                    DataOrdem = dataReferencia,
                    Ticker = ticker,
                    TipoOrdem = TipoOrdem.LotePadrao,
                    Quantidade = qtdLote,
                    PrecoUnitario = cotacao,
                    ValorTotal = qtdLote * cotacao,
                    Status = StatusOrdem.Executada,
                    ContaGraficaId = contaMaster.Id
                };
                ordensCompra.Add(ordem);
                ordensResponse.Add(new OrdemCompraResponse(
                    ticker, "LOTE_PADRAO", qtdLote, cotacao, qtdLote * cotacao));
            }

            // Fracionário (1-99)
            if (fracionario > 0)
            {
                var ordem = new OrdemCompra
                {
                    DataOrdem = dataReferencia,
                    Ticker = $"{ticker}F",
                    TipoOrdem = TipoOrdem.Fracionario,
                    Quantidade = fracionario,
                    PrecoUnitario = cotacao,
                    ValorTotal = fracionario * cotacao,
                    Status = StatusOrdem.Executada,
                    ContaGraficaId = contaMaster.Id
                };
                ordensCompra.Add(ordem);
                ordensResponse.Add(new OrdemCompraResponse(
                    $"{ticker}F", "FRACIONARIO", fracionario, cotacao, fracionario * cotacao));
            }
        }

        // Salvar ordens
        if (ordensCompra.Any())
        {
            await _ordemCompraRepository.CriarVariasAsync(ordensCompra);
        }

        // Calcular total disponível por ticker (compras + saldo master)
        var totalDisponivel = new Dictionary<string, int>();
        foreach (var ticker in tickers)
        {
            var comprado = quantidadePorTicker.GetValueOrDefault(ticker, 0);
            var saldoMaster = custodiaMaster.ContainsKey(ticker) ? custodiaMaster[ticker].Quantidade : 0;
            totalDisponivel[ticker] = comprado + saldoMaster;
        }

        // Distribuição proporcional para cada cliente
        var distribuicoes = new List<DistribuicaoClienteResponse>();
        var distribuicaoPorCliente = new Dictionary<long, Dictionary<string, int>>();

        foreach (var item in clientesComAportes)
        {
            var proporcao = item.AporteData / totalConsolidado;
            var ativosDistribuidos = new Dictionary<string, int>();

            foreach (var ticker in tickers)
            {
                var qtdProporcional = (int)Math.Truncate(totalDisponivel[ticker] * proporcao);
                ativosDistribuidos[ticker] = qtdProporcional;
            }

            distribuicaoPorCliente[item.Cliente.Id] = ativosDistribuidos;
        }

        // Calcular resíduos (ajustar se soma ultrapassar disponível)
        var residuos = new List<ResiduoResponse>();
        foreach (var ticker in tickers)
        {
            var distribuido = distribuicaoPorCliente.Values.Sum(d => d.GetValueOrDefault(ticker, 0));
            var residuo = totalDisponivel[ticker] - distribuido;
            if (residuo > 0)
            {
                residuos.Add(new ResiduoResponse(ticker, residuo));
            }
        }

        // Atualizar custódia de cada cliente e criar histórico
        foreach (var item in clientesComAportes)
        {
            var contaCliente = await _contaGraficaRepository.ObterPorClienteIdAsync(item.Cliente.Id);
            if (contaCliente == null) continue;

            var ativos = new List<DistribuicaoAtivoResponse>();
            var distribuicoesAporte = new List<DistribuicaoAporte>();

            foreach (var ticker in tickers)
            {
                var quantidade = distribuicaoPorCliente[item.Cliente.Id].GetValueOrDefault(ticker, 0);
                if (quantidade == 0) continue;

                var cotacao = cotacoes[ticker];
                var valor = quantidade * cotacao;

                // Atualizar custódia do cliente
                var custodiaCliente = await _custodiaRepository.ObterPorContaETickerAsync(contaCliente.Id, ticker);
                if (custodiaCliente == null)
                {
                    custodiaCliente = new Custodia
                    {
                        ContaGraficaId = contaCliente.Id,
                        Ticker = ticker,
                        Quantidade = quantidade,
                        PrecoMedio = cotacao,
                        DataAtualizacao = dataReferencia
                    };
                    await _custodiaRepository.CriarAsync(custodiaCliente);
                }
                else
                {
                    // Recalcular preço médio
                    var qtdAnterior = custodiaCliente.Quantidade;
                    var pmAnterior = custodiaCliente.PrecoMedio;
                    var novoPrecoMedio = ((qtdAnterior * pmAnterior) + (quantidade * cotacao)) / (qtdAnterior + quantidade);

                    custodiaCliente.Quantidade += quantidade;
                    custodiaCliente.PrecoMedio = novoPrecoMedio;
                    custodiaCliente.DataAtualizacao = dataReferencia;
                    await _custodiaRepository.AtualizarAsync(custodiaCliente);
                }

                ativos.Add(new DistribuicaoAtivoResponse(ticker, quantidade, cotacao, valor));
                distribuicoesAporte.Add(new DistribuicaoAporte
                {
                    Ticker = ticker,
                    QuantidadeComprada = quantidade,
                    PrecoUnitario = cotacao,
                    ValorTotal = valor
                });
            }

            // Criar histórico de aporte
            var historicoAporte = new HistoricoAporte
            {
                ClienteId = item.Cliente.Id,
                DataAporte = dataReferencia,
                ValorAporte = item.AporteData,
                Distribuicoes = distribuicoesAporte
            };
            await _historicoAporteRepository.CriarAsync(historicoAporte);

            distribuicoes.Add(new DistribuicaoClienteResponse(
                item.Cliente.Id,
                item.Cliente.Nome,
                contaCliente.NumeroConta,
                item.AporteData,
                ativos
            ));
        }

        // Atualizar custódia master com resíduos
        foreach (var residuo in residuos)
        {
            var custodia = await _custodiaRepository.ObterPorContaETickerAsync(contaMaster.Id, residuo.Ticker);
            if (custodia == null)
            {
                custodia = new Custodia
                {
                    ContaGraficaId = contaMaster.Id,
                    Ticker = residuo.Ticker,
                    Quantidade = residuo.Quantidade,
                    PrecoMedio = cotacoes[residuo.Ticker],
                    DataAtualizacao = dataReferencia
                };
                await _custodiaRepository.CriarAsync(custodia);
            }
            else
            {
                // Custódia master: resíduos são o novo saldo
                custodia.Quantidade = residuo.Quantidade;
                custodia.DataAtualizacao = dataReferencia;
                await _custodiaRepository.AtualizarAsync(custodia);
            }
        }

        // Zerar saldo de ativos que foram totalmente distribuídos
        foreach (var ticker in tickers)
        {
            if (!residuos.Any(r => r.Ticker == ticker))
            {
                var custodia = await _custodiaRepository.ObterPorContaETickerAsync(contaMaster.Id, ticker);
                if (custodia != null && custodia.Quantidade > 0)
                {
                    custodia.Quantidade = 0;
                    custodia.DataAtualizacao = dataReferencia;
                    await _custodiaRepository.AtualizarAsync(custodia);
                }
            }
        }

        // Eventos IR (simulado - em produção publicaria no Kafka)
        var eventosIR = distribuicoes.Sum(d => d.Ativos.Count);

        return new ExecutarCompraResponse(
            DataExecucao: dataReferencia,
            TotalClientes: clientesAtivos.Count,
            TotalConsolidado: Math.Round(totalConsolidado, 2),
            OrdensCompra: ordensResponse,
            Distribuicoes: distribuicoes,
            ResiduosCustMaster: residuos,
            EventosIRPublicados: eventosIR,
            Mensagem: $"Compra programada executada com sucesso para {clientesAtivos.Count} clientes."
        );
    }

    private static DateOnly AjustarParaDiaUtil(DateOnly data)
    {
        // Se for sábado, ajusta para segunda
        if (data.DayOfWeek == DayOfWeek.Saturday)
            return data.AddDays(2);

        // Se for domingo, ajusta para segunda
        if (data.DayOfWeek == DayOfWeek.Sunday)
            return data.AddDays(1);

        return data;
    }
}
