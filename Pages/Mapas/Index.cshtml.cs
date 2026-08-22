using GeoPharma.Data;
using GeoPharma.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace GeoPharma.Pages.Mapas;

public class IndexModel : PageModel
{
    private readonly AppDbContext _context;

    public IndexModel(AppDbContext context)
    {
        _context = context;
    }

    public IList<PontoMapaViewModel> Pontos { get; set; }
        = new List<PontoMapaViewModel>();

    public int TotalEstabelecimentos { get; set; }
    public int TotalOportunidades { get; set; }
    public int TotalLeads { get; set; }
    public int TotalEmAndamento { get; set; }
    public int TotalNegociando { get; set; }
    public int TotalConvertidos { get; set; }

    public async Task OnGetAsync()
    {
        var estabelecimentos = await _context.Clientes
            .AsNoTracking()
            .Where(e =>
                e.Ativo &&
                e.Latitude.HasValue &&
                e.Longitude.HasValue)
            .ToListAsync();

        var leads = await _context.Leads
            .AsNoTracking()
            .ToListAsync();

        var pontos = new List<PontoMapaViewModel>();

        /*
         * ============================================================
         * ESTABELECIMENTOS
         * ============================================================
         *
         * Um estabelecimento representa uma oportunidade de mercado.
         *
         * Se já existir um Lead correspondente, ele não será exibido
         * novamente como oportunidade.
         */
        foreach (var estabelecimento in estabelecimentos)
        {
            var leadCorrespondente = EncontrarLeadCorrespondente(
                estabelecimento,
                leads);

            if (leadCorrespondente != null)
            {
                continue;
            }

            pontos.Add(new PontoMapaViewModel
            {
                Tipo = "oportunidade",

                EstabelecimentoId = estabelecimento.Id,

                Nome = ObterNomeEstabelecimento(estabelecimento),

                Cnpj = string.IsNullOrWhiteSpace(estabelecimento.Cnpj)
                    ? "Não informado"
                    : estabelecimento.Cnpj,

                Endereco = MontarEndereco(estabelecimento),

                Bairro = estabelecimento.Bairro ?? "",

                Cidade = estabelecimento.Cidade ?? "",

                Uf = estabelecimento.Uf ?? "",

                Regiao = estabelecimento.Regiao ?? "",

                Latitude = estabelecimento.Latitude!.Value,

                Longitude = estabelecimento.Longitude!.Value,

                Status = "Não capturado",

                Responsavel = "",

                DataCaptura = ""
            });
        }

        /*
         * ============================================================
         * LEADS
         * ============================================================
         */
        foreach (var lead in leads)
        {
            pontos.Add(new PontoMapaViewModel
            {
                Tipo = "lead",

                LeadId = lead.Id,

                Nome = lead.Nome,

                Cnpj = string.IsNullOrWhiteSpace(lead.Cnpj)
                    ? "Não informado"
                    : lead.Cnpj,

                Endereco = lead.Endereco,

                Latitude = lead.Latitude,

                Longitude = lead.Longitude,

                Status = NormalizarStatus(lead.Status),

                Responsavel =
                    string.IsNullOrWhiteSpace(lead.VendedorResponsavel)
                        ? "Não informado"
                        : lead.VendedorResponsavel,

                DataCaptura =
                    lead.DataCriacao.ToString("dd/MM/yyyy HH:mm")
            });
        }

        Pontos = pontos;

        /*
         * ============================================================
         * INDICADORES
         * ============================================================
         */
        TotalEstabelecimentos = estabelecimentos.Count;

        TotalOportunidades =
            Pontos.Count(p => p.Tipo == "oportunidade");

        TotalLeads =
            Pontos.Count(p => p.Tipo == "lead");

        TotalEmAndamento =
            Pontos.Count(p =>
                p.Tipo == "lead" &&
                NormalizarStatus(p.Status) == "Em Andamento");

        TotalNegociando =
            Pontos.Count(p =>
                p.Tipo == "lead" &&
                NormalizarStatus(p.Status) == "Negociando");

        TotalConvertidos =
            Pontos.Count(p =>
                p.Tipo == "lead" &&
                NormalizarStatus(p.Status) == "Convertido");
    }

    /*
     * ================================================================
     * CAPTURA UM ESTABELECIMENTO COMO LEAD
     * ================================================================
     */
    public async Task<IActionResult> OnPostCapturarLeadAsync(
        [FromBody] CapturarLeadInputModel model)
    {
        if (model == null || model.EstabelecimentoId <= 0)
        {
            return new JsonResult(new
            {
                success = false,
                message = "Estabelecimento inválido."
            });
        }

        var estabelecimento =
            await _context.Clientes
                .FirstOrDefaultAsync(e =>
                    e.Id == model.EstabelecimentoId &&
                    e.Ativo);

        if (estabelecimento == null)
        {
            return new JsonResult(new
            {
                success = false,
                message = "Estabelecimento não encontrado."
            });
        }

        /*
         * ------------------------------------------------------------
         * DUPLICIDADE POR CNPJ
         * ------------------------------------------------------------
         */
        if (!string.IsNullOrWhiteSpace(estabelecimento.Cnpj))
        {
            var cnpjNormalizado =
                NormalizarCnpj(estabelecimento.Cnpj);

            var leadsExistentes =
                await _context.Leads
                    .Where(l => l.Cnpj != null)
                    .ToListAsync();

            var duplicadoCnpj =
                leadsExistentes.Any(l =>
                    NormalizarCnpj(l.Cnpj) == cnpjNormalizado);

            if (duplicadoCnpj)
            {
                return new JsonResult(new
                {
                    success = false,
                    message =
                        "Este CNPJ já está cadastrado na carteira de leads."
                });
            }
        }

        /*
         * ------------------------------------------------------------
         * DUPLICIDADE POR NOME + ENDEREÇO
         * ------------------------------------------------------------
         */
        var nome =
            ObterNomeEstabelecimento(estabelecimento);

        var endereco =
            MontarEndereco(estabelecimento);

        var nomeNormalizado =
            NormalizarTexto(nome);

        var enderecoNormalizado =
            NormalizarTexto(endereco);

        var leadsParaComparacao =
            await _context.Leads.ToListAsync();

        var duplicado =
            leadsParaComparacao.Any(l =>
                NormalizarTexto(l.Nome) == nomeNormalizado &&
                NormalizarTexto(l.Endereco) == enderecoNormalizado);

        if (duplicado)
        {
            return new JsonResult(new
            {
                success = false,
                message =
                    "Este estabelecimento já foi capturado anteriormente."
            });
        }

        /*
         * ------------------------------------------------------------
         * RESPONSÁVEL
         * ------------------------------------------------------------
         *
         * Quando a autenticação real estiver ativa,
         * User.Identity.Name identificará o vendedor.
         *
         * O fallback evita quebrar o sistema atual.
         */
        var responsavel =
            ObterUsuarioAtual();

        var lead = new Lead
        {
            Nome = nome,

            Cnpj = estabelecimento.Cnpj,

            Endereco = endereco,

            Latitude = estabelecimento.Latitude!.Value,

            Longitude = estabelecimento.Longitude!.Value,

            Status = "Em Andamento",

            VendedorResponsavel = responsavel,

            DataCriacao = DateTime.Now
        };

        _context.Leads.Add(lead);

        await _context.SaveChangesAsync();

        return new JsonResult(new
        {
            success = true,

            message =
                $"Lead capturado com sucesso por {responsavel}.",

            lead = new
            {
                id = lead.Id,
                nome = lead.Nome,
                cnpj = string.IsNullOrWhiteSpace(lead.Cnpj)
                    ? "Não informado"
                    : lead.Cnpj,
                endereco = lead.Endereco,
                latitude = lead.Latitude,
                longitude = lead.Longitude,
                status = lead.Status,
                responsavel = lead.VendedorResponsavel,
                dataCaptura =
                    lead.DataCriacao.ToString("dd/MM/yyyy HH:mm")
            }
        });
    }

    /*
     * ================================================================
     * ALTERAÇÃO DE STATUS DIRETAMENTE PELO MAPA
     * ================================================================
     */
    public async Task<IActionResult> OnPostAtualizarStatusAsync(
        [FromBody] AtualizarStatusInputModel model)
    {
        if (model == null || model.LeadId <= 0)
        {
            return new JsonResult(new
            {
                success = false,
                message = "Lead inválido."
            });
        }

        var statusPermitidos = new[]
        {
            "Em Andamento",
            "Negociando",
            "Convertido",
            "Perdido"
        };

        var novoStatus =
            statusPermitidos.FirstOrDefault(s =>
                string.Equals(
                    s,
                    model.Status?.Trim(),
                    StringComparison.OrdinalIgnoreCase));

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
                    l.Id == model.LeadId);

        if (lead == null)
        {
            return new JsonResult(new
            {
                success = false,
                message = "Lead não encontrado."
            });
        }

        lead.Status = novoStatus;

        await _context.SaveChangesAsync();

        return new JsonResult(new
        {
            success = true,
            message = "Status atualizado com sucesso.",
            status = novoStatus
        });
    }

    /*
     * ================================================================
     * MÉTODOS AUXILIARES
     * ================================================================
     */

    private Lead? EncontrarLeadCorrespondente(
        Cliente estabelecimento,
        IEnumerable<Lead> leads)
    {
        /*
         * Primeiro critério: CNPJ.
         */
        if (!string.IsNullOrWhiteSpace(estabelecimento.Cnpj))
        {
            var cnpj =
                NormalizarCnpj(estabelecimento.Cnpj);

            var leadPorCnpj =
                leads.FirstOrDefault(l =>
                    !string.IsNullOrWhiteSpace(l.Cnpj) &&
                    NormalizarCnpj(l.Cnpj) == cnpj);

            if (leadPorCnpj != null)
            {
                return leadPorCnpj;
            }
        }

        /*
         * Segundo critério: nome + endereço.
         */
        var nome =
            NormalizarTexto(
                ObterNomeEstabelecimento(estabelecimento));

        var endereco =
            NormalizarTexto(
                MontarEndereco(estabelecimento));

        return leads.FirstOrDefault(l =>
            NormalizarTexto(l.Nome) == nome &&
            NormalizarTexto(l.Endereco) == endereco);
    }

    private string ObterUsuarioAtual()
    {
        var usuario =
            User.Identity?.Name;

        if (string.IsNullOrWhiteSpace(usuario))
        {
            return "Admin";
        }

        if (usuario.Contains("@"))
        {
            usuario =
                usuario.Split('@')[0];
        }

        if (string.IsNullOrWhiteSpace(usuario))
        {
            return "Admin";
        }

        return char.ToUpper(usuario[0]) +
               usuario.Substring(1);
    }

    private static string ObterNomeEstabelecimento(
        Cliente estabelecimento)
    {
        if (!string.IsNullOrWhiteSpace(
                estabelecimento.NomeFantasia))
        {
            return estabelecimento.NomeFantasia;
        }

        if (!string.IsNullOrWhiteSpace(
                estabelecimento.RazaoSocial))
        {
            return estabelecimento.RazaoSocial;
        }

        return $"Estabelecimento #{estabelecimento.Id}";
    }

    private static string MontarEndereco(
        Cliente estabelecimento)
    {
        var partes = new List<string>();

        if (!string.IsNullOrWhiteSpace(
                estabelecimento.Logradouro))
        {
            var endereco =
                estabelecimento.Logradouro;

            if (!string.IsNullOrWhiteSpace(
                    estabelecimento.Numero))
            {
                endereco +=
                    $", {estabelecimento.Numero}";
            }

            partes.Add(endereco);
        }

        if (!string.IsNullOrWhiteSpace(
                estabelecimento.Bairro))
        {
            partes.Add(estabelecimento.Bairro);
        }

        var cidadeUf = "";

        if (!string.IsNullOrWhiteSpace(
                estabelecimento.Cidade))
        {
            cidadeUf = estabelecimento.Cidade;
        }

        if (!string.IsNullOrWhiteSpace(
                estabelecimento.Uf))
        {
            cidadeUf +=
                string.IsNullOrWhiteSpace(cidadeUf)
                    ? estabelecimento.Uf
                    : $" - {estabelecimento.Uf}";
        }

        if (!string.IsNullOrWhiteSpace(cidadeUf))
        {
            partes.Add(cidadeUf);
        }

        if (!string.IsNullOrWhiteSpace(
                estabelecimento.Cep))
        {
            partes.Add($"CEP {estabelecimento.Cep}");
        }

        return partes.Count > 0
            ? string.Join(" | ", partes)
            : "Endereço não informado";
    }

    private static string NormalizarStatus(
        string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return "Em Andamento";
        }

        if (status.Equals(
            "Em andamento",
            StringComparison.OrdinalIgnoreCase))
        {
            return "Em Andamento";
        }

        if (status.Equals(
            "Negociando",
            StringComparison.OrdinalIgnoreCase))
        {
            return "Negociando";
        }

        if (status.Equals(
            "Convertido",
            StringComparison.OrdinalIgnoreCase))
        {
            return "Convertido";
        }

        if (status.Equals(
            "Perdido",
            StringComparison.OrdinalIgnoreCase))
        {
            return "Perdido";
        }

        return status.Trim();
    }

    private static string NormalizarCnpj(
        string? cnpj)
    {
        if (string.IsNullOrWhiteSpace(cnpj))
        {
            return "";
        }

        return Regex.Replace(
            cnpj,
            @"\D",
            "");
    }

    private static string NormalizarTexto(
        string? texto)
    {
        return (texto ?? "")
            .Trim()
            .ToUpperInvariant();
    }
}


/*
 * ===================================================================
 * VIEW MODEL DO MAPA
 * ===================================================================
 */
public class PontoMapaViewModel
{
    public string Tipo { get; set; } = "";

    public int? EstabelecimentoId { get; set; }

    public int? LeadId { get; set; }

    public string Nome { get; set; } = "";

    public string Cnpj { get; set; } = "";

    public string Endereco { get; set; } = "";

    public string Bairro { get; set; } = "";

    public string Cidade { get; set; } = "";

    public string Uf { get; set; } = "";

    public string Regiao { get; set; } = "";

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public string Status { get; set; } = "";

    public string Responsavel { get; set; } = "";

    public string DataCaptura { get; set; } = "";
}


public class CapturarLeadInputModel
{
    public int EstabelecimentoId { get; set; }
}


public class AtualizarStatusInputModel
{
    public int LeadId { get; set; }

    public string Status { get; set; } = "";
}