namespace Domain.Entities;

public class DistribuicaoAporte
{
    public long Id { get; set; }
    public long HistoricoAporteId { get; set; }
    public string Ticker { get; set; } = string.Empty;
    public int QuantidadeComprada { get; set; }
    public decimal PrecoUnitario { get; set; }
    public decimal ValorTotal { get; set; }

    // Navegação
    public HistoricoAporte HistoricoAporte { get; set; } = null!;
}
