using GeoPharma.Data;
using GeoPharma.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GeoPharma.Pages.Account;

public class LoginModel : PageModel
{
    private readonly AppDbContext _context;

    public LoginModel(AppDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? MensagemErro { get; set; }

    public class InputModel
    {
        public string Email { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(Input.Email) ||
            string.IsNullOrWhiteSpace(Input.Senha))
        {
            MensagemErro = "Preencha todos os campos.";
            return Page();
        }

        var email = Input.Email.Trim().ToLowerInvariant();

        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email);

        if (usuario == null)
        {
            MensagemErro = "E-mail ou senha inválidos.";
            return Page();
        }

        var passwordHasher = new PasswordHasher<Usuario>();

        var resultado = PasswordVerificationResult.Failed;

        try
        {
            resultado = passwordHasher.VerifyHashedPassword(
                usuario,
                usuario.SenhaHash,
                Input.Senha);
        }
        catch
        {
            // Compatibilidade com usuários antigos
            // cuja senha ainda esteja armazenada sem hash.
        }

        var senhaValida =
            resultado == PasswordVerificationResult.Success ||
            resultado == PasswordVerificationResult.SuccessRehashNeeded;

        // Compatibilidade temporária com usuários antigos
        if (!senhaValida && usuario.SenhaHash == Input.Senha)
        {
            usuario.SenhaHash =
                passwordHasher.HashPassword(usuario, Input.Senha);

            await _context.SaveChangesAsync();

            senhaValida = true;
        }

        if (!senhaValida)
        {
            MensagemErro = "E-mail ou senha inválidos.";
            return Page();
        }

        return RedirectToPage("/Index");
    }
}