using GeoPharma.Data;
using GeoPharma.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GeoPharma.Pages.Clientes
{
    public class CreateModel : PageModel
    {
        private readonly AppDbContext _context;

        public CreateModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Cliente Cliente { get; set; } = new();

        public IActionResult OnGet()
        {
            Cliente.Ativo = true;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var cnpjNormalizado = NormalizarCnpj(Cliente.Cnpj);

            if (string.IsNullOrWhiteSpace(cnpjNormalizado))
            {
                ModelState.AddModelError(
                    "Cliente.Cnpj",
                    "Informe um CNPJ válido."
                );

                return Page();
            }

            if (cnpjNormalizado.Length != 14)
            {
                ModelState.AddModelError(
                    "Cliente.Cnpj",
                    "O CNPJ deve possuir 14 números."
                );

                return Page();
            }

            /*
             * Verifica duplicidade.
             *
             * Como registros antigos podem estar armazenados
             * com máscara, fazemos a comparação normalizando
             * os CNPJs existentes.
             */
            var clientesExistentes =
                await _context.Clientes
                    .AsNoTracking()
                    .Where(c => c.Cnpj != null)
                    .Select(c => new
                    {
                        c.Id,
                        c.Cnpj,
                        c.NomeFantasia,
                        c.RazaoSocial
                    })
                    .ToListAsync();

            var clienteDuplicado =
                clientesExistentes
                    .FirstOrDefault(c =>
                        NormalizarCnpj(c.Cnpj) ==
                        cnpjNormalizado
                    );

            if (clienteDuplicado != null)
            {
                var nomeCliente =
                    !string.IsNullOrWhiteSpace(
                        clienteDuplicado.NomeFantasia
                    )
                        ? clienteDuplicado.NomeFantasia
                        : clienteDuplicado.RazaoSocial;

                ModelState.AddModelError(
                    "Cliente.Cnpj",
                    $"Este CNPJ já está cadastrado" +
                    (
                        string.IsNullOrWhiteSpace(nomeCliente)
                            ? "."
                            : $" para o cliente \"{nomeCliente}\"."
                    )
                );

                return Page();
            }

            /*
             * Padroniza o CNPJ antes de salvar.
             */
            Cliente.Cnpj =
                FormatarCnpj(cnpjNormalizado);

            _context.Clientes.Add(Cliente);

            await _context.SaveChangesAsync();

            TempData["MensagemSucesso"] =
                $"Cliente {ObterNomeCliente(Cliente)} cadastrado com sucesso.";

            return RedirectToPage("./Index");
        }

        private static string NormalizarCnpj(string? cnpj)
        {
            if (string.IsNullOrWhiteSpace(cnpj))
            {
                return string.Empty;
            }

            return new string(
                cnpj
                    .Where(char.IsDigit)
                    .ToArray()
            );
        }

        private static string FormatarCnpj(string cnpj)
        {
            if (cnpj.Length != 14)
            {
                return cnpj;
            }

            return
                $"{cnpj[..2]}." +
                $"{cnpj.Substring(2, 3)}." +
                $"{cnpj.Substring(5, 3)}/" +
                $"{cnpj.Substring(8, 4)}-" +
                $"{cnpj.Substring(12, 2)}";
        }

        private static string ObterNomeCliente(
            Cliente cliente
        )
        {
            if (!string.IsNullOrWhiteSpace(
                cliente.NomeFantasia
            ))
            {
                return cliente.NomeFantasia;
            }

            if (!string.IsNullOrWhiteSpace(
                cliente.RazaoSocial
            ))
            {
                return cliente.RazaoSocial;
            }

            return "Cliente";
        }
    }
}