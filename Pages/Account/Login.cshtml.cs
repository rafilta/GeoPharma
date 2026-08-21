using GeoPharma.Data;
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
        // Se os campos vierem vazios
        if (string.IsNullOrWhiteSpace(Input.Email) || string.IsNullOrWhiteSpace(Input.Senha))
        {
            MensagemErro = "Preencha todos os campos.";
            return Page();
        }

        // Busca o usuário no MySQL sem diferenciar maiúsculas/minúsculas
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Email.ToLower() == Input.Email.ToLower());

        // Validação da credencial
        if (usuario == null || usuario.SenhaHash != Input.Senha)
        {
            MensagemErro = "E-mail ou senha inválidos.";
            return Page();
        }

        // Sucesso -> Redireciona para o mapa/dashboard
        return RedirectToPage("/Index");
    }
}