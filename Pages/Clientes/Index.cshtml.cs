using GeoPharma.Data;
using GeoPharma.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GeoPharma.Pages.Clientes
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public IList<Cliente> Clientes { get; set; }
            = new List<Cliente>();

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        public async Task OnGetAsync()
        {
            var query =
                _context.Clientes
                    .AsNoTracking()
                    .AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                query = query.Where(c =>
                    (c.NomeFantasia != null &&
                     c.NomeFantasia.Contains(SearchTerm)) ||

                    (c.RazaoSocial != null &&
                     c.RazaoSocial.Contains(SearchTerm)) ||

                    (c.Cnpj != null &&
                     c.Cnpj.Contains(SearchTerm)) ||

                    (c.Bairro != null &&
                     c.Bairro.Contains(SearchTerm)) ||

                    (c.Regiao != null &&
                     c.Regiao.Contains(SearchTerm)) ||

                    (c.Cidade != null &&
                     c.Cidade.Contains(SearchTerm)));
            }

            Clientes =
                await query
                    .OrderBy(c => c.NomeFantasia)
                    .ToListAsync();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var cliente =
                await _context.Clientes
                    .FindAsync(id);

            if (cliente != null)
            {
                _context.Clientes.Remove(cliente);

                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }
    }
}