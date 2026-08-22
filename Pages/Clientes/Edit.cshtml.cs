using GeoPharma.Data;
using GeoPharma.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GeoPharma.Pages.Clientes
{
    public class EditModel : PageModel
    {
        private readonly AppDbContext _context;

        public EditModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Cliente Cliente { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(
            int? id
        )
        {
            if (id == null)
            {
                return NotFound();
            }

            var cliente =
                await _context.Clientes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c =>
                        c.Id == id
                    );

            if (cliente == null)
            {
                return NotFound();
            }

            Cliente = cliente;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var clienteBanco =
                await _context.Clientes
                    .FirstOrDefaultAsync(c =>
                        c.Id == Cliente.Id
                    );

            if (clienteBanco == null)
            {
                return NotFound();
            }

            var cnpjNormalizado =
                NormalizarCnpj(Cliente.Cnpj);

            if (string.IsNullOrWhiteSpace(
                cnpjNormalizado
            ))
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
             * Procura o mesmo CNPJ em OUTRO cliente.
             */
            var outrosClientes =
                await _context.Clientes
                    .AsNoTracking()
                    .Where(c =>
                        c.Id != Cliente.Id &&
                        c.Cnpj != null
                    )
                    .Select(c => new
                    {
                        c.Id,
                        c.Cnpj,
                        c.NomeFantasia,
                        c.RazaoSocial
                    })
                    .ToListAsync();

            var clienteDuplicado =
                outrosClientes
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
                    $"Este CNPJ já pertence" +
                    (
                        string.IsNullOrWhiteSpace(nomeCliente)
                            ? " a outro cliente."
                            : $" ao cliente \"{nomeCliente}\"."
                    )
                );

                return Page();
            }

            /*
             * Atualiza somente os campos permitidos.
             */
            clienteBanco.Regiao =
                Cliente.Regiao;

            clienteBanco.NomeFantasia =
                Cliente.NomeFantasia;

            clienteBanco.RazaoSocial =
                Cliente.RazaoSocial;

            clienteBanco.Cnpj =
                FormatarCnpj(
                    cnpjNormalizado
                );

            clienteBanco.Cep =
                Cliente.Cep;

            clienteBanco.Logradouro =
                Cliente.Logradouro;

            clienteBanco.Numero =
                Cliente.Numero;

            clienteBanco.Bairro =
                Cliente.Bairro;

            clienteBanco.Cidade =
                Cliente.Cidade;

            clienteBanco.Uf =
                Cliente.Uf;

            clienteBanco.Latitude =
                Cliente.Latitude;

            clienteBanco.Longitude =
                Cliente.Longitude;

            clienteBanco.Ativo =
                Cliente.Ativo;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClienteExists(
                    Cliente.Id
                ))
                {
                    return NotFound();
                }

                throw;
            }

            TempData["MensagemSucesso"] =
                $"Cliente {ObterNomeCliente(clienteBanco)} atualizado com sucesso.";

            return RedirectToPage("./Index");
        }

        private bool ClienteExists(int id)
        {
            return _context.Clientes
                .Any(c =>
                    c.Id == id
                );
        }

        private static string NormalizarCnpj(
            string? cnpj
        )
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

        private static string FormatarCnpj(
            string cnpj
        )
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