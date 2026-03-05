using Application.Common;
using Application.DTOs.Requests;
using Application.DTOs.Responses;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services;

public class ClienteService : IClienteService
{
    private readonly IClienteRepository _clienteRepository;
    private readonly IContaGraficaRepository _contaGraficaRepository;
    private readonly ICustodiaRepository _custodiaRepository;
    private readonly IHistoricoAporteRepository _historicoAporteRepository;
    private readonly ICotacaoService _cotacaoService;

    private const decimal VALOR_MENSAL_MINIMO = 100m;

    public ClienteService(
        IClienteRepository clienteRepository,
        IContaGraficaRepository contaGraficaRepository,
        ICustodiaRepository custodiaRepository,
        IHistoricoAporteRepository historicoAporteRepository,
        ICotacaoService cotacaoService)
    {
        _clienteRepository = clienteRepository;
        _contaGraficaRepository = contaGraficaRepository;
        _custodiaRepository = custodiaRepository;
        _historicoAporteRepository = historicoAporteRepository;
        _cotacaoService = cotacaoService;
    }

    public async Task<Result<AdesaoResponse>> AderirAsync(AdesaoRequest request)
    {
        // Validação: Formato do CPF
        if (!ValidarCpf(request.CPF))
        {
            return Error.Validation("CPF invalido. Deve conter 11 digitos numericos validos.", "CPF_INVALIDO");
        }

        // Validação: CPF duplicado
        if (await _clienteRepository.ExistePorCpfAsync(request.CPF))
        {
            return Error.Validation("CPF ja cadastrado no sistema.", "CLIENTE_CPF_DUPLICADO");
        }

        // Validação: Valor mínimo
        if (request.ValorMensal < VALOR_MENSAL_MINIMO)
        {
            return Error.Validation($"O valor mensal minimo e de R$ {VALOR_MENSAL_MINIMO:F2}.", "VALOR_MENSAL_INVALIDO");
        }

        var dataAdesao = DateTime.UtcNow;

        // Criar cliente
        var cliente = new Cliente
        {
            Nome = request.Nome,
            CPF = request.CPF,
            Email = request.Email,
            ValorMensal = request.ValorMensal,
            Ativo = true,
            DataAdesao = dataAdesao
        };

        await _clienteRepository.CriarAsync(cliente);

        // Criar conta gráfica filhote
        var numeroConta = await _contaGraficaRepository.GerarProximoNumeroContaFilhoteAsync();
        var contaGrafica = new ContaGrafica
        {
            NumeroConta = numeroConta,
            Tipo = TipoConta.Filhote,
            DataCriacao = dataAdesao,
            ClienteId = cliente.Id
        };

        await _contaGraficaRepository.CriarAsync(contaGrafica);

        return new AdesaoResponse(
            ClienteId: cliente.Id,
            Nome: cliente.Nome,
            CPF: cliente.CPF,
            Email: cliente.Email,
            ValorMensal: cliente.ValorMensal,
            Ativo: cliente.Ativo,
            DataAdesao: cliente.DataAdesao,
            ContaGrafica: new ContaGraficaResponse(
                Id: contaGrafica.Id,
                NumeroConta: contaGrafica.NumeroConta,
                Tipo: contaGrafica.Tipo.ToString().ToUpper(),
                DataCriacao: contaGrafica.DataCriacao
            )
        );
    }

    public async Task<Result<SaidaResponse>> SairAsync(long clienteId)
    {
        var cliente = await _clienteRepository.ObterPorIdAsync(clienteId);

        if (cliente == null)
        {
            return Error.NotFound("Cliente nao encontrado.", "CLIENTE_NAO_ENCONTRADO");
        }

        if (!cliente.Ativo)
        {
            return Error.Validation("Cliente ja havia saido do produto.", "CLIENTE_JA_INATIVO");
        }

        var dataSaida = DateTime.UtcNow;
        cliente.Ativo = false;
        cliente.DataSaida = dataSaida;

        await _clienteRepository.AtualizarAsync(cliente);

        return new SaidaResponse(
            ClienteId: cliente.Id,
            Nome: cliente.Nome,
            Ativo: false,
            DataSaida: dataSaida,
            Mensagem: "Adesao encerrada. Sua posicao em custodia foi mantida."
        );
    }

    public async Task<Result<AlterarValorMensalResponse>> AlterarValorMensalAsync(long clienteId, AlterarValorMensalRequest request)
    {
        var cliente = await _clienteRepository.ObterPorIdAsync(clienteId);

        if (cliente == null)
        {
            return Error.NotFound("Cliente nao encontrado.", "CLIENTE_NAO_ENCONTRADO");
        }

        if (!cliente.Ativo)
        {
            return Error.Validation("Cliente inativo nao pode alterar valor mensal.", "CLIENTE_JA_INATIVO");
        }

        if (request.NovoValorMensal < VALOR_MENSAL_MINIMO)
        {
            return Error.Validation($"O valor mensal minimo e de R$ {VALOR_MENSAL_MINIMO:F2}.", "VALOR_MENSAL_INVALIDO");
        }

        var valorAnterior = cliente.ValorMensal;
        var dataAlteracao = DateTime.UtcNow;

        cliente.ValorMensal = request.NovoValorMensal;
        await _clienteRepository.AtualizarAsync(cliente);

        return new AlterarValorMensalResponse(
            ClienteId: cliente.Id,
            ValorMensalAnterior: valorAnterior,
            ValorMensalNovo: request.NovoValorMensal,
            DataAlteracao: dataAlteracao,
            Mensagem: "Valor mensal atualizado. O novo valor sera considerado a partir da proxima data de compra."
        );
    }

    public async Task<Result<CarteiraResponse>> ConsultarCarteiraAsync(long clienteId)
    {
        var cliente = await _clienteRepository.ObterPorIdComContaGraficaAsync(clienteId);

        if (cliente == null)
        {
            return Error.NotFound("Cliente nao encontrado.", "CLIENTE_NAO_ENCONTRADO");
        }

        var contaGrafica = cliente.ContaGrafica;
        if (contaGrafica == null)
        {
            return Error.NotFound("Conta grafica nao encontrada.", "CONTA_NAO_ENCONTRADA");
        }

        var custodias = await _custodiaRepository.ObterPorContaGraficaIdAsync(contaGrafica.Id);
        var tickers = custodias.Select(c => c.Ticker);
        var cotacoes = await _cotacaoService.ObterCotacoesAsync(tickers);

        var ativos = new List<AtivoCarteiraResponse>();
        decimal valorTotalInvestido = 0;
        decimal valorAtualCarteira = 0;

        foreach (var custodia in custodias)
        {
            var cotacaoAtual = cotacoes.GetValueOrDefault(custodia.Ticker, custodia.PrecoMedio);
            var valorInvestido = custodia.Quantidade * custodia.PrecoMedio;
            var valorAtual = custodia.Quantidade * cotacaoAtual;
            var pl = valorAtual - valorInvestido;
            var plPercentual = valorInvestido > 0 ? (pl / valorInvestido) * 100 : 0;

            valorTotalInvestido += valorInvestido;
            valorAtualCarteira += valorAtual;

            ativos.Add(new AtivoCarteiraResponse(
                Ticker: custodia.Ticker,
                Quantidade: custodia.Quantidade,
                PrecoMedio: Math.Round(custodia.PrecoMedio, 2),
                CotacaoAtual: Math.Round(cotacaoAtual, 2),
                ValorAtual: Math.Round(valorAtual, 2),
                PL: Math.Round(pl, 2),
                PLPercentual: Math.Round(plPercentual, 2),
                ComposicaoCarteira: 0 // Será calculado após somar tudo
            ));
        }

        // Calcular composição da carteira
        var ativosComComposicao = ativos.Select(a => a with
        {
            ComposicaoCarteira = valorAtualCarteira > 0
                ? Math.Round((a.ValorAtual / valorAtualCarteira) * 100, 2)
                : 0
        }).ToList();

        var plTotal = valorAtualCarteira - valorTotalInvestido;
        var rentabilidade = valorTotalInvestido > 0
            ? Math.Round((plTotal / valorTotalInvestido) * 100, 2)
            : 0;

        return new CarteiraResponse(
            ClienteId: cliente.Id,
            Nome: cliente.Nome,
            ContaGrafica: contaGrafica.NumeroConta,
            DataConsulta: DateTime.UtcNow,
            Resumo: new ResumoCarteiraResponse(
                ValorTotalInvestido: Math.Round(valorTotalInvestido, 2),
                ValorAtualCarteira: Math.Round(valorAtualCarteira, 2),
                PLTotal: Math.Round(plTotal, 2),
                RentabilidadePercentual: rentabilidade
            ),
            Ativos: ativosComComposicao
        );
    }

    public async Task<Result<RentabilidadeResponse>> ConsultarRentabilidadeAsync(long clienteId)
    {
        var cliente = await _clienteRepository.ObterPorIdComContaGraficaAsync(clienteId);

        if (cliente == null)
        {
            return Error.NotFound("Cliente nao encontrado.", "CLIENTE_NAO_ENCONTRADO");
        }

        var carteiraResult = await ConsultarCarteiraAsync(clienteId);
        if (carteiraResult.IsFailure)
        {
            return carteiraResult.Error!;
        }
        var carteira = carteiraResult.Value!;
        var historicoAportes = await _historicoAporteRepository.ObterPorClienteIdAsync(clienteId);

        var aportes = historicoAportes.Select(h => new AporteResponse(
            Data: h.DataAporte,
            Valor: h.ValorAporte,
            Distribuicao: h.Distribuicoes.Select(d => new DistribuicaoAporteResponse(
                Ticker: d.Ticker,
                Quantidade: d.QuantidadeComprada,
                PrecoUnitario: d.PrecoUnitario,
                Valor: d.ValorTotal
            )).ToList()
        )).ToList();

        // Calcular evolução da carteira (simplificado - acumulado por aporte)
        var evolucao = new List<EvolucaoCarteiraResponse>();
        decimal valorAcumulado = 0;

        foreach (var aporte in aportes.OrderBy(a => a.Data))
        {
            valorAcumulado += aporte.Valor;
            // Para uma evolução mais precisa, seria necessário consultar cotações históricas
            evolucao.Add(new EvolucaoCarteiraResponse(
                Data: aporte.Data,
                ValorInvestido: valorAcumulado,
                ValorCarteira: valorAcumulado, // Simplificado
                PL: 0,
                RentabilidadePercentual: 0
            ));
        }

        // Adicionar situação atual
        if (carteira.Resumo.ValorTotalInvestido > 0)
        {
            evolucao.Add(new EvolucaoCarteiraResponse(
                Data: DateTime.UtcNow,
                ValorInvestido: carteira.Resumo.ValorTotalInvestido,
                ValorCarteira: carteira.Resumo.ValorAtualCarteira,
                PL: carteira.Resumo.PLTotal,
                RentabilidadePercentual: carteira.Resumo.RentabilidadePercentual
            ));
        }

        return new RentabilidadeResponse(
            ClienteId: cliente.Id,
            Nome: cliente.Nome,
            DataConsulta: DateTime.UtcNow,
            Rentabilidade: carteira.Resumo,
            HistoricoAportes: aportes,
            EvolucaoCarteira: evolucao
        );
    }

    /// <summary>
    /// Valida o CPF usando o algoritmo oficial da Receita Federal.
    /// </summary>
    private static bool ValidarCpf(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            return false;

        // Remove caracteres não numéricos
        var numeros = new string(cpf.Where(char.IsDigit).ToArray());

        // Deve ter exatamente 11 dígitos
        if (numeros.Length != 11)
            return false;

        // Verifica se todos os dígitos são iguais (ex: 111.111.111-11)
        if (numeros.Distinct().Count() == 1)
            return false;

        // Calcula primeiro dígito verificador
        var soma = 0;
        for (var i = 0; i < 9; i++)
            soma += (numeros[i] - '0') * (10 - i);

        var resto = soma % 11;
        var digito1 = resto < 2 ? 0 : 11 - resto;

        if (numeros[9] - '0' != digito1)
            return false;

        // Calcula segundo dígito verificador
        soma = 0;
        for (var i = 0; i < 10; i++)
            soma += (numeros[i] - '0') * (11 - i);

        resto = soma % 11;
        var digito2 = resto < 2 ? 0 : 11 - resto;

        return numeros[10] - '0' == digito2;
    }
}
