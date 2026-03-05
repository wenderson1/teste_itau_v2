namespace Domain.Entities;

public class Cesta
{
    public long Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public bool Ativa { get; set; } = true;
    public DateTime DataCriacao { get; set; }
    public DateTime? DataDesativacao { get; set; }

    // Navegação
    public ICollection<ItemCesta> Itens { get; set; } = new List<ItemCesta>();
}
