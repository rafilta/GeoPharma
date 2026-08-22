using GeoPharma.Data;
using GeoPharma.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

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
        public Cliente Cliente { get; set; }
            = new Cliente();

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

            _context.Clientes.Add(Cliente);

            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}