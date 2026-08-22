using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using GeoPharma.Data;
using GeoPharma.Models;

namespace GeoPharma.Pages.Mapas
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public IList<Estabelecimento> Estabelecimentos { get; set; } = default!;
        public IList<LeadCapturadoViewModel> LeadsSalvos { get; set; } = new List<LeadCapturadoViewModel>();

        public async Task OnGetAsync()
        {
            Estabelecimentos = await _context.Estabelecimentos
                .Where(e => e.Ativo && e.Latitude != null && e.Longitude != null)
                .ToListAsync();

            LeadsSalvos = await _context.Leads
                .Select(l => new LeadCapturadoViewModel
                {
                    Nome = l.Nome,
                    Endereco = l.Endereco,
                    Latitude = l.Latitude,
                    Longitude = l.Longitude,
                    Cnpj = string.IsNullOrEmpty(l.Cnpj) ? "Não informado" : l.Cnpj,
                    Status = string.IsNullOrEmpty(l.Status) ? "Em Andamento" : l.Status,
                    Responsavel = string.IsNullOrEmpty(l.VendedorResponsavel) ? "Sistema" : l.VendedorResponsavel,
                    DataCaptura = l.DataCriacao.ToString("dd/MM/yyyy HH:mm")
                })
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostSalvarLeadAsync([FromBody] LeadInputModel model)
        {
            if (model == null || string.IsNullOrEmpty(model.Nome))
            {
                return new JsonResult(new { success = false, message = "Dados inválidos." });
            }

            // Validação estrita por CNPJ se informado
            if (!string.IsNullOrEmpty(model.Cnpj))
            {
                bool existeCnpj = await _context.Leads.AnyAsync(l => l.Cnpj == model.Cnpj);
                if (existeCnpj)
                {
                    return new JsonResult(new { success = false, message = "Este CNPJ já está cadastrado na base de leads!" });
                }
            }

            bool existe = await _context.Leads
                .AnyAsync(l => l.Nome.ToLower() == model.Nome.ToLower() && l.Endereco.ToLower() == model.Endereco.ToLower());

            if (existe)
            {
                return new JsonResult(new { success = false, message = "Este estabelecimento já foi capturado anteriormente!" });
            }

            // Identificação dinâmica do usuário logado (Admin, Rafael, Gabriel, etc.)
            string usuarioAtual = User.Identity?.Name ?? "Usuário Anônimo";

            if (usuarioAtual.Contains("@"))
            {
                usuarioAtual = usuarioAtual.Split('@')[0];
                usuarioAtual = char.ToUpper(usuarioAtual[0]) + usuarioAtual.Substring(1);
            }

            var novoLead = new Lead
            {
                Nome = model.Nome,
                Endereco = model.Endereco,
                Cnpj = model.Cnpj,
                Latitude = model.Latitude,
                Longitude = model.Longitude,
                DataCriacao = DateTime.Now,
                VendedorResponsavel = usuarioAtual,
                Status = "Em Andamento"
            };

            _context.Leads.Add(novoLead);
            await _context.SaveChangesAsync();

            return new JsonResult(new { success = true, message = $"Lead capturado com sucesso por {usuarioAtual}!" });
        }
    }

    public class LeadInputModel
    {
        public string Nome { get; set; } = string.Empty;
        public string Endereco { get; set; } = string.Empty;
        public string? Cnpj { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public class LeadCapturadoViewModel
    {
        public string Nome { get; set; } = string.Empty;
        public string Endereco { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Cnpj { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Responsavel { get; set; } = string.Empty;
        public string DataCaptura { get; set; } = string.Empty;
    }
}