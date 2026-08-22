using System.ComponentModel.DataAnnotations;

namespace GeoPharma.Models;

public class Regiao
{
    public int Id { get; set; }

    [Required(ErrorMessage = "O nome da região é obrigatório.")]
    [StringLength(100)]
    public string Nome { get; set; } = string.Empty;

    [StringLength(250)]
    public string? Descricao { get; set; }

    // Relacionamento 1 para Muitos (Uma região possui vários estabelecimentos)
    public ICollection<Cliente> Estabelecimentos { get; set; } = new List<Cliente>();
}