namespace Domain.Entities;

public enum TipoOrdem
{
    LotePadrao,
    Fracionario
}

public enum StatusOrdem
{
    Pendente,
    Executada,
    Cancelada
}

public class OrdemCompra
{
    public long Id { get; set; }
    public DateTime DataOrdem { get; set; }
    public string Ticker { get; set; } = string.Empty;
    public TipoOrdem TipoOrdem { get; set; }
    public int Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }
    public decimal ValorTotal { get; set; }
    public StatusOrdem Status { get; set; }

    // FK para conta master
    public long ContaGraficaId { get; set; }
    public ContaGrafica ContaGrafica { get; set; } = null!;
}
