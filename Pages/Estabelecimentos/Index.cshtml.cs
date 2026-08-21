using GeoPharma.Data;
using GeoPharma.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GeoPharma.Pages.Estabelecimentos
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public IList<Estabelecimento> Estabelecimentos { get; set; } = default!;

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        public async Task OnGetAsync()
        {
            var query = _context.Estabelecimentos.AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                query = query.Where(e => (e.NomeFantasia != null && e.NomeFantasia.Contains(SearchTerm)) ||
                                         (e.RazaoSocial != null && e.RazaoSocial.Contains(SearchTerm)) ||
                                         (e.Cnpj != null && e.Cnpj.Contains(SearchTerm)) ||
                                         (e.Bairro != null && e.Bairro.Contains(SearchTerm)) ||
                                         (e.Regiao != null && e.Regiao.Contains(SearchTerm)) ||
                                         (e.Cidade != null && e.Cidade.Contains(SearchTerm)));
            }

            Estabelecimentos = await query.ToListAsync();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var estabelecimento = await _context.Estabelecimentos.FindAsync(id);

            if (estabelecimento != null)
            {
                _context.Estabelecimentos.Remove(estabelecimento);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }
    }
}