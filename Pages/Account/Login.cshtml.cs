using GeoPharma.Data;
using GeoPharma.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace GeoPharma.Pages.Account;

[EnableRateLimiting("login")]
public class LoginModel : PageModel
{
    private readonly AppDbContext _context;

    public LoginModel(AppDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public string? MensagemErro { get; set; }

    public class InputModel
    {
        public string Email { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
    }

    public IActionResult OnGet()
    {
        if (User.Identity?.IsAuthenticated == true)
            return LocalRedirect(Url.IsLocalUrl(ReturnUrl) ? ReturnUrl! : "/");

        return Page();
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

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.Nome),
            new(ClaimTypes.GivenName, usuario.Nome.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? usuario.Nome),
            new(ClaimTypes.Email, usuario.Email),
            new(ClaimTypes.Role, usuario.Tipo.ToString())
        };

        var principal = new ClaimsPrincipal(
            new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = false,
                AllowRefresh = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            });

        return LocalRedirect(Url.IsLocalUrl(ReturnUrl) ? ReturnUrl! : "/");
    }
}
