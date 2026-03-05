namespace Domain.Entities;

public enum TipoConta
{
    Master,
    Filhote
}

public class ContaGrafica
{
    public long Id { get; set; }
    public string NumeroConta { get; set; } = string.Empty;
    public TipoConta Tipo { get; set; }
    public DateTime DataCriacao { get; set; }

    // FK opcional para cliente (null para conta Master)
    public long? ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    // Navegação
    public ICollection<Custodia> Custodias { get; set; } = new List<Custodia>();
}
