using GeoPharma.Data;
using GeoPharma.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;

namespace GeoPharma.Pages.Mapas
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public IndexModel(
            AppDbContext context,
            IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public string GoogleMapsApiKey { get; set; } = "";

        public IList<ClienteMapaViewModel> ClientesMapa { get; set; }
            = new List<ClienteMapaViewModel>();

        public IList<LeadMapaViewModel> LeadsMapa { get; set; }
            = new List<LeadMapaViewModel>();

        public int TotalClientes { get; set; }
        public int TotalLeads { get; set; }
        public int TotalEmAndamento { get; set; }
        public int TotalNegociando { get; set; }

        // ============================================================
        // CARREGAMENTO DO MAPA
        // ============================================================

        public async Task OnGetAsync()
        {
            GoogleMapsApiKey =
                _configuration["GoogleMaps:ApiKey"] ?? "";

            // --------------------------------------------------------
            // CLIENTES
            // --------------------------------------------------------

            var clientes = await _context.Clientes
                .AsNoTracking()
                .Where(c =>
                    c.Latitude.HasValue &&
                    c.Longitude.HasValue)
                .OrderBy(c => c.NomeFantasia)
                .ToListAsync();

            ClientesMapa = clientes
                .Select(c => new ClienteMapaViewModel
                {
                    Id = c.Id,

                    Nome = ObterNomeCliente(c),

                    RazaoSocial =
                        string.IsNullOrWhiteSpace(c.RazaoSocial)
                            ? "Não informada"
                            : c.RazaoSocial,

                    Cnpj =
                        string.IsNullOrWhiteSpace(c.Cnpj)
                            ? "Não informado"
                            : c.Cnpj,

                    Endereco =
                        MontarEnderecoCliente(c),

                    Regiao =
                        c.Regiao ?? "",

                    Latitude =
                        c.Latitude!.Value,

                    Longitude =
                        c.Longitude!.Value,

                    Ativo =
                        c.Ativo
                })
                .ToList();

            // --------------------------------------------------------
            // LEADS
            // --------------------------------------------------------

            var leads = await _context.Leads
                .AsNoTracking()
                .OrderByDescending(l => l.DataCriacao)
                .ToListAsync();

            LeadsMapa = leads
                .Select(l => new LeadMapaViewModel
                {
                    Id = l.Id,

                    Nome = l.Nome,

                    Cnpj =
                        string.IsNullOrWhiteSpace(l.Cnpj)
                            ? "Não informado"
                            : l.Cnpj,

                    Endereco =
                        string.IsNullOrWhiteSpace(l.Endereco)
                            ? "Endereço não informado"
                            : l.Endereco,

                    Latitude =
                        l.Latitude,

                    Longitude =
                        l.Longitude,

                    Status =
                        string.IsNullOrWhiteSpace(l.Status)
                            ? "Em Andamento"
                            : l.Status,

                    Responsavel =
                        string.IsNullOrWhiteSpace(l.VendedorResponsavel)
                            ? "Não informado"
                            : l.VendedorResponsavel,

                    DataCaptura =
                        l.DataCriacao.ToString(
                            "dd/MM/yyyy HH:mm"
                        )
                })
                .ToList();

            // --------------------------------------------------------
            // KPIs
            // --------------------------------------------------------

            TotalClientes =
                await _context.Clientes.CountAsync();

            TotalLeads =
                await _context.Leads.CountAsync();

            TotalEmAndamento =
                await _context.Leads.CountAsync(l =>
                    l.Status == "Em Andamento" ||
                    l.Status == "Em andamento");

            TotalNegociando =
                await _context.Leads.CountAsync(l =>
                    l.Status == "Negociando");
        }

        // ============================================================
        // CAPTURAR LEAD
        // ============================================================

        public async Task<IActionResult> OnPostCapturarLeadAsync(
            [FromBody] CapturarLeadMapaInput model)
        {
            if (model == null)
            {
                return new JsonResult(new
                {
                    success = false,
                    message = "Dados inválidos."
                });
            }

            if (string.IsNullOrWhiteSpace(model.Nome))
            {
                return new JsonResult(new
                {
                    success = false,
                    message = "O estabelecimento não possui nome válido."
                });
            }

            if (model.Latitude == 0 ||
                model.Longitude == 0)
            {
                return new JsonResult(new
                {
                    success = false,
                    message = "O estabelecimento não possui localização válida."
                });
            }

            // --------------------------------------------------------
            // BLOQUEIA CLIENTE EXISTENTE
            // --------------------------------------------------------

            var clientes =
                await _context.Clientes
                    .AsNoTracking()
                    .Where(c =>
                        c.Latitude.HasValue &&
                        c.Longitude.HasValue)
                    .ToListAsync();

            foreach (var cliente in clientes)
            {
                if (!string.IsNullOrWhiteSpace(model.Cnpj) &&
                    !string.IsNullOrWhiteSpace(cliente.Cnpj))
                {
                    var cnpjNovo =
                        SomenteNumeros(model.Cnpj);

                    var cnpjCliente =
                        SomenteNumeros(cliente.Cnpj);

                    if (cnpjNovo.Length == 14 &&
                        cnpjNovo == cnpjCliente)
                    {
                        return new JsonResult(new
                        {
                            success = false,

                            message =
                                $"Este CNPJ já pertence ao cliente " +
                                $"{ObterNomeCliente(cliente)}."
                        });
                    }
                }

                var distancia =
                    CalcularDistanciaMetros(
                        model.Latitude,
                        model.Longitude,
                        cliente.Latitude!.Value,
                        cliente.Longitude!.Value
                    );

                if (distancia <= 50)
                {
                    return new JsonResult(new
                    {
                        success = false,

                        message =
                            $"Existe um cliente cadastrado neste local: " +
                            $"{ObterNomeCliente(cliente)}."
                    });
                }
            }

            // --------------------------------------------------------
            // BLOQUEIA LEAD EXISTENTE
            // --------------------------------------------------------

            var leads =
                await _context.Leads
                    .AsNoTracking()
                    .ToListAsync();

            var nomeNormalizado =
                NormalizarTexto(model.Nome);

            foreach (var leadExistente in leads)
            {
                if (!string.IsNullOrWhiteSpace(model.Cnpj) &&
                    !string.IsNullOrWhiteSpace(leadExistente.Cnpj))
                {
                    var cnpjNovo =
                        SomenteNumeros(model.Cnpj);

                    var cnpjLead =
                        SomenteNumeros(leadExistente.Cnpj);

                    if (cnpjNovo.Length == 14 &&
                        cnpjNovo == cnpjLead)
                    {
                        return new JsonResult(new
                        {
                            success = false,

                            message =
                                $"Este CNPJ já foi capturado por " +
                                $"{leadExistente.VendedorResponsavel ?? "outro representante"}."
                        });
                    }
                }

                var distancia =
                    CalcularDistanciaMetros(
                        model.Latitude,
                        model.Longitude,
                        leadExistente.Latitude,
                        leadExistente.Longitude
                    );

                if (distancia <= 50)
                {
                    return new JsonResult(new
                    {
                        success = false,

                        message =
                            $"Este ponto já foi capturado por " +
                            $"{leadExistente.VendedorResponsavel ?? "outro representante"}."
                    });
                }

                var mesmoNome =
                    NormalizarTexto(
                        leadExistente.Nome
                    ) == nomeNormalizado;

                if (mesmoNome &&
                    distancia <= 200)
                {
                    return new JsonResult(new
                    {
                        success = false,

                        message =
                            $"Já existe um lead para " +
                            $"{leadExististenteNome(leadExistente)}."
                    });
                }
            }

            // --------------------------------------------------------
            // CRIAÇÃO
            // --------------------------------------------------------

            var responsavel =
                ObterUsuarioAtual();

            var lead = new Lead
            {
                Nome =
                    model.Nome.Trim(),

                Cnpj =
                    string.IsNullOrWhiteSpace(model.Cnpj)
                        ? null
                        : model.Cnpj.Trim(),

                Endereco =
                    string.IsNullOrWhiteSpace(model.Endereco)
                        ? "Endereço não informado"
                        : model.Endereco.Trim(),

                Latitude =
                    model.Latitude,

                Longitude =
                    model.Longitude,

                Status =
                    "Em Andamento",

                VendedorResponsavel =
                    responsavel,

                DataCriacao =
                    DateTime.Now
            };

            _context.Leads.Add(lead);

            await _context.SaveChangesAsync();

            return new JsonResult(new
            {
                success = true,

                message =
                    $"Lead capturado por {responsavel}.",

                lead = new
                {
                    id =
                        lead.Id,

                    nome =
                        lead.Nome,

                    cnpj =
                        string.IsNullOrWhiteSpace(lead.Cnpj)
                            ? "Não informado"
                            : lead.Cnpj,

                    endereco =
                        lead.Endereco,

                    latitude =
                        lead.Latitude,

                    longitude =
                        lead.Longitude,

                    status =
                        lead.Status,

                    responsavel =
                        lead.VendedorResponsavel,

                    dataCaptura =
                        lead.DataCriacao.ToString(
                            "dd/MM/yyyy HH:mm"
                        )
                }
            });
        }

        // ============================================================
        // STATUS
        // ============================================================

        public async Task<IActionResult> OnPostAtualizarStatusAsync(
            [FromBody] AtualizarStatusMapaInput model)
        {
            var permitidos = new[]
            {
                "Em Andamento",
                "Negociando",
                "Convertido",
                "Perdido"
            };

            if (model == null ||
                model.LeadId <= 0)
            {
                return new JsonResult(new
                {
                    success = false,
                    message = "Lead inválido."
                });
            }

            var novoStatus =
                permitidos.FirstOrDefault(s =>
                    string.Equals(
                        s,
                        model.Status,
                        StringComparison.OrdinalIgnoreCase
                    )
                );

            if (novoStatus == null)
            {
                return new JsonResult(new
                {
                    success = false,
                    message = "Status inválido."
                });
            }

            var lead =
                await _context.Leads
                    .FirstOrDefaultAsync(l =>
                        l.Id == model.LeadId
                    );

            if (lead == null)
            {
                return new JsonResult(new
                {
                    success = false,
                    message = "Lead não encontrado."
                });
            }

            lead.Status =
                novoStatus;

            await _context.SaveChangesAsync();

            return new JsonResult(new
            {
                success = true,
                status = novoStatus,
                message = "Status atualizado."
            });
        }

        // ============================================================
        // AUXILIARES
        // ============================================================

        private string ObterUsuarioAtual()
        {
            var nome =
                User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(nome))
            {
                return "Admin";
            }

            if (nome.Contains("@"))
            {
                nome =
                    nome.Split('@')[0];
            }

            return nome;
        }

        private static string leadExististenteNome(
            Lead lead)
        {
            return string.IsNullOrWhiteSpace(lead.Nome)
                ? "lead existente"
                : lead.Nome;
        }

        private static string ObterNomeCliente(
            Cliente cliente)
        {
            if (!string.IsNullOrWhiteSpace(
                cliente.NomeFantasia))
            {
                return cliente.NomeFantasia;
            }

            if (!string.IsNullOrWhiteSpace(
                cliente.RazaoSocial))
            {
                return cliente.RazaoSocial;
            }

            return $"Cliente #{cliente.Id}";
        }

        private static string MontarEnderecoCliente(
            Cliente cliente)
        {
            var partes =
                new List<string>();

            if (!string.IsNullOrWhiteSpace(
                cliente.Logradouro))
            {
                var linha =
                    cliente.Logradouro;

                if (!string.IsNullOrWhiteSpace(
                    cliente.Numero))
                {
                    linha +=
                        $", {cliente.Numero}";
                }

                partes.Add(linha);
            }

            if (!string.IsNullOrWhiteSpace(
                cliente.Bairro))
            {
                partes.Add(
                    cliente.Bairro
                );
            }

            if (!string.IsNullOrWhiteSpace(
                cliente.Cidade))
            {
                var cidade =
                    cliente.Cidade;

                if (!string.IsNullOrWhiteSpace(
                    cliente.Uf))
                {
                    cidade +=
                        $" - {cliente.Uf}";
                }

                partes.Add(cidade);
            }

            if (!string.IsNullOrWhiteSpace(
                cliente.Cep))
            {
                partes.Add(
                    $"CEP {cliente.Cep}"
                );
            }

            return partes.Count == 0
                ? "Endereço não informado"
                : string.Join(" | ", partes);
        }

        private static string SomenteNumeros(
            string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return "";
            }

            return new string(
                valor
                    .Where(char.IsDigit)
                    .ToArray()
            );
        }

        private static string NormalizarTexto(
            string? texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
            {
                return "";
            }

            texto =
                texto
                    .Trim()
                    .ToUpperInvariant()
                    .Normalize(
                        NormalizationForm.FormD
                    );

            var chars =
                texto.Where(c =>
                    CharUnicodeInfo
                        .GetUnicodeCategory(c) !=
                    UnicodeCategory.NonSpacingMark
                );

            return new string(chars.ToArray())
                .Normalize(
                    NormalizationForm.FormC
                );
        }

        private static double CalcularDistanciaMetros(
            double lat1,
            double lon1,
            double lat2,
            double lon2)
        {
            const double raioTerra =
                6371000;

            var latitude1 =
                GrausParaRadianos(lat1);

            var latitude2 =
                GrausParaRadianos(lat2);

            var deltaLatitude =
                GrausParaRadianos(
                    lat2 - lat1
                );

            var deltaLongitude =
                GrausParaRadianos(
                    lon2 - lon1
                );

            var a =
                Math.Sin(deltaLatitude / 2) *
                Math.Sin(deltaLatitude / 2) +

                Math.Cos(latitude1) *
                Math.Cos(latitude2) *

                Math.Sin(deltaLongitude / 2) *
                Math.Sin(deltaLongitude / 2);

            var c =
                2 *
                Math.Atan2(
                    Math.Sqrt(a),
                    Math.Sqrt(1 - a)
                );

            return raioTerra * c;
        }

        private static double GrausParaRadianos(
            double graus)
        {
            return graus *
                   Math.PI /
                   180;
        }
    }

    // ================================================================
    // VIEW MODELS
    // ================================================================

    public class ClienteMapaViewModel
    {
        public int Id { get; set; }

        public string Nome { get; set; } = "";

        public string RazaoSocial { get; set; } = "";

        public string Cnpj { get; set; } = "";

        public string Endereco { get; set; } = "";

        public string Regiao { get; set; } = "";

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public bool Ativo { get; set; }
    }

    public class LeadMapaViewModel
    {
        public int Id { get; set; }

        public string Nome { get; set; } = "";

        public string Cnpj { get; set; } = "";

        public string Endereco { get; set; } = "";

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public string Status { get; set; } = "";

        public string Responsavel { get; set; } = "";

        public string DataCaptura { get; set; } = "";
    }

    public class CapturarLeadMapaInput
    {
        public string Nome { get; set; } = "";

        public string? RazaoSocial { get; set; }

        public string? Cnpj { get; set; }

        public string Endereco { get; set; } = "";

        public double Latitude { get; set; }

        public double Longitude { get; set; }
    }

    public class AtualizarStatusMapaInput
    {
        public int LeadId { get; set; }

        public string Status { get; set; } = "";
    }
}