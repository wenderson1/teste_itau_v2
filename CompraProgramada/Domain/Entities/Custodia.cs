namespace Domain.Entities;

public class Custodia
{
    public long Id { get; set; }
    public long ContaGraficaId { get; set; }
    public string Ticker { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal PrecoMedio { get; set; }
    public DateTime DataAtualizacao { get; set; }

    // Navegação
    public ContaGrafica ContaGrafica { get; set; } = null!;
}
