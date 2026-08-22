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
        public Cliente Cliente { get; set; }
            = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cliente =
                await _context.Clientes
                    .FirstOrDefaultAsync(c =>
                        c.Id == id);

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

            _context.Attach(Cliente)
                .State =
                EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }

            catch (DbUpdateConcurrencyException)
            {
                if (!ClienteExists(Cliente.Id))
                {
                    return NotFound();
                }

                throw;
            }

            return RedirectToPage("./Index");
        }

        private bool ClienteExists(int id)
        {
            return _context.Clientes
                .Any(c => c.Id == id);
        }
    }
}