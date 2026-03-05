namespace Domain.Entities;

public class HistoricoAporte
{
    public long Id { get; set; }
    public long ClienteId { get; set; }
    public DateTime DataAporte { get; set; }
    public decimal ValorAporte { get; set; }

    // Navegação
    public Cliente Cliente { get; set; } = null!;
    public ICollection<DistribuicaoAporte> Distribuicoes { get; set; } = new List<DistribuicaoAporte>();
}
