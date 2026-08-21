using GeoPharma.Enums;
using System.ComponentModel.DataAnnotations;

namespace GeoPharma.Models;

public class Usuario
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Nome { get; set; } = string.Empty;

    [Required, MaxLength(150), EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MaxLength(255)]
    public string SenhaHash { get; set; } = string.Empty;

    public TipoUsuario Tipo { get; set; } = TipoUsuario.Vendedor;

    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
}