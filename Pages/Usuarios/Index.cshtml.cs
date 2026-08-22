using GeoPharma.Data;
using GeoPharma.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GeoPharma.Pages.Usuarios;

public class IndexModel : PageModel
{
    private readonly AppDbContext _context;

    public IndexModel(AppDbContext context)
    {
        _context = context;
    }

    public IList<Usuario> Usuarios { get; set; } = new List<Usuario>();

    public async Task OnGetAsync()
    {
        Usuarios = await _context.Usuarios
            .AsNoTracking()
            .OrderBy(u => u.Nome)
            .ToListAsync();
    }
}