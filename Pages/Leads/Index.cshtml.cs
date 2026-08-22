using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using GeoPharma.Data;
using GeoPharma.Models;

namespace GeoPharma.Pages.Leads;

public class IndexModel : PageModel
{
    private readonly AppDbContext _context;

    public IndexModel(AppDbContext context)
    {
        _context = context;
    }

    public IList<Lead> Leads { get; set; } = default!;

    public async Task OnGetAsync()
    {
        Leads = await _context.Leads.OrderByDescending(l => l.DataCriacao).ToListAsync();
    }
}