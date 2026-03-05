namespace Domain.Entities;

public class ItemCesta
{
    public long Id { get; set; }
    public long CestaId { get; set; }
    public string Ticker { get; set; } = string.Empty;
    public decimal Percentual { get; set; }

    // Navegação
    public Cesta Cesta { get; set; } = null!;
}
