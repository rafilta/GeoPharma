using GeoPharma.Data;
using GeoPharma.Enums;
using GeoPharma.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GeoPharma.Pages.Usuarios;

public class CreateModel : PageModel
{
    private readonly AppDbContext _context;

    public CreateModel(AppDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required(ErrorMessage = "Informe o nome.")]
        [MaxLength(100)]
        [Display(Name = "Nome")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe o e-mail.")]
        [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
        [MaxLength(150)]
        [Display(Name = "E-mail")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe a senha.")]
        [MinLength(6, ErrorMessage = "A senha deve possuir pelo menos 6 caracteres.")]
        [DataType(DataType.Password)]
        [Display(Name = "Senha")]
        public string Senha { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirme a senha.")]
        [DataType(DataType.Password)]
        [Compare(nameof(Senha), ErrorMessage = "As senhas não conferem.")]
        [Display(Name = "Confirmar senha")]
        public string ConfirmarSenha { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Tipo de usuário")]
        public TipoUsuario Tipo { get; set; } = TipoUsuario.Vendedor;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var email = Input.Email.Trim().ToLowerInvariant();

        var emailJaExiste = await _context.Usuarios
            .AnyAsync(u => u.Email.ToLower() == email);

        if (emailJaExiste)
        {
            ModelState.AddModelError(
                "Input.Email",
                "Já existe um usuário cadastrado com este e-mail.");

            return Page();
        }

        var usuario = new Usuario
        {
            Nome = Input.Nome.Trim(),
            Email = email,
            Tipo = Input.Tipo,
            CriadoEm = DateTime.UtcNow
        };

        var passwordHasher = new PasswordHasher<Usuario>();

        usuario.SenhaHash = passwordHasher.HashPassword(
            usuario,
            Input.Senha);

        _context.Usuarios.Add(usuario);

        await _context.SaveChangesAsync();

        TempData["MensagemSucesso"] =
            $"Usuário {usuario.Nome} cadastrado com sucesso.";

        return RedirectToPage("./Index");
    }
}