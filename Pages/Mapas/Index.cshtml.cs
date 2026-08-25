using GeoPharma.Data;
using GeoPharma.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace GeoPharma.Pages.Mapas
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        private static readonly HttpClient Http = CriarHttpClient();

        private static readonly SemaphoreSlim NominatimLock =
            new(1, 1);

        private static DateTime UltimaChamadaNominatim =
            DateTime.MinValue;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public IList<ClienteMapaVm> ClientesMapa { get; set; }
            = new List<ClienteMapaVm>();

        public IList<LeadMapaVm> LeadsMapa { get; set; }
            = new List<LeadMapaVm>();

        public int TotalClientes { get; set; }
        public int TotalLeads { get; set; }
        public int TotalEmAndamento { get; set; }
        public int TotalNegociando { get; set; }

        public async Task OnGetAsync()
        {
            var clientes =
                await _context.Clientes
                    .AsNoTracking()
                    .Where(c =>
                        c.Latitude.HasValue &&
                        c.Longitude.HasValue)
                    .ToListAsync();

            ClientesMapa =
                clientes.Select(c =>
                    new ClienteMapaVm
                    {
                        Id = c.Id,
                        Nome = ObterNomeCliente(c),
                        RazaoSocial = c.RazaoSocial ?? "",
                        Cnpj = c.Cnpj ?? "",
                        Endereco = MontarEndereco(
                            c.Logradouro,
                            c.Numero,
                            c.Bairro,
                            c.Cidade,
                            c.Uf,
                            c.Cep),
                        Latitude = c.Latitude!.Value,
                        Longitude = c.Longitude!.Value
                    })
                    .ToList();

            var leads =
                await _context.Leads
                    .AsNoTracking()
                    .OrderByDescending(l => l.DataCriacao)
                    .ToListAsync();

            LeadsMapa =
                leads.Select(l =>
                    new LeadMapaVm
                    {
                        Id = l.Id,
                        Nome = l.Nome,
                        Cnpj = l.Cnpj ?? "",
                        Endereco = l.Endereco ?? "",
                        Latitude = l.Latitude,
                        Longitude = l.Longitude,
                        Status = string.IsNullOrWhiteSpace(l.Status)
                            ? "Em Andamento"
                            : l.Status,
                        Responsavel =
                            l.VendedorResponsavel ??
                            "Não informado",
                        DataCaptura =
                            l.DataCriacao.ToString(
                                "dd/MM/yyyy HH:mm")
                    })
                    .ToList();

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

        // =========================================================
        // ENDEREÇOS POSSÍVEIS
        // =========================================================

        public async Task<IActionResult>
            OnGetBuscarEnderecosAsync(string termo)
        {
            if (string.IsNullOrWhiteSpace(termo))
            {
                return new JsonResult(new
                {
                    success = false,
                    message = "Digite um endereço."
                });
            }

            var consulta =
                termo.Trim();

            if (!consulta.Contains(
                "Brasil",
                StringComparison.OrdinalIgnoreCase))
            {
                consulta += ", Brasil";
            }

            var url =
                "https://nominatim.openstreetmap.org/search" +
                "?format=jsonv2" +
                "&addressdetails=1" +
                "&countrycodes=br" +
                "&limit=8" +
                "&q=" +
                Uri.EscapeDataString(consulta);

            var doc =
                await ConsultarNominatimAsync(url);

            if (doc == null ||
                doc.RootElement.ValueKind !=
                JsonValueKind.Array)
            {
                return new JsonResult(new
                {
                    success = false,
                    message =
                        "Não foi possível consultar endereços."
                });
            }

            var resultados =
                new List<EnderecoPesquisaVm>();

            foreach (var item
                in doc.RootElement.EnumerateArray())
            {
                var lat =
                    ParseDouble(
                        JsonString(item, "lat"));

                var lon =
                    ParseDouble(
                        JsonString(item, "lon"));

                if (!lat.HasValue ||
                    !lon.HasValue)
                {
                    continue;
                }

                var cidade = "";
                var uf = "";

                if (item.TryGetProperty(
                    "address",
                    out var address))
                {
                    cidade =
                        ExtrairCidade(address);

                    uf =
                        ExtrairUf(address);
                }

                resultados.Add(
                    new EnderecoPesquisaVm
                    {
                        Endereco =
                            JsonString(
                                item,
                                "display_name"),

                        Latitude =
                            lat.Value,

                        Longitude =
                            lon.Value,

                        Cidade =
                            cidade,

                        Uf =
                            uf
                    });
            }

            return new JsonResult(new
            {
                success = true,
                data = resultados
            });
        }

        // =========================================================
        // CONTEXTO DO GPS
        // =========================================================

        public async Task<IActionResult>
            OnGetContextoAsync(
                double latitude,
                double longitude)
        {
            var url =
                "https://nominatim.openstreetmap.org/reverse" +
                "?format=jsonv2" +
                "&addressdetails=1" +
                "&zoom=18" +
                "&lat=" +
                latitude.ToString(
                    CultureInfo.InvariantCulture) +
                "&lon=" +
                longitude.ToString(
                    CultureInfo.InvariantCulture);

            var doc =
                await ConsultarNominatimAsync(url);

            if (doc == null)
            {
                return new JsonResult(new
                {
                    success = false,
                    message =
                        "Não foi possível identificar sua localização."
                });
            }

            if (!doc.RootElement.TryGetProperty(
                "address",
                out var address))
            {
                return new JsonResult(new
                {
                    success = false,
                    message =
                        "Município não identificado."
                });
            }

            var cidade =
                ExtrairCidade(address);

            var uf =
                ExtrairUf(address);

            if (string.IsNullOrWhiteSpace(cidade) ||
                string.IsNullOrWhiteSpace(uf))
            {
                return new JsonResult(new
                {
                    success = false,
                    message =
                        "Cidade ou UF não identificadas."
                });
            }

            return new JsonResult(new
            {
                success = true,

                data = new
                {
                    cidade,
                    uf,

                    endereco =
                        JsonString(
                            doc.RootElement,
                            "display_name")
                }
            });
        }

        // =========================================================
        // POSSÍVEIS LEADS
        // =========================================================

        public async Task<IActionResult>
            OnGetPossiveisAsync(
                double latitude,
                double longitude,
                double raioKm,
                string cidade,
                string uf)
        {
            if (raioKm < 1)
            {
                raioKm = 1;
            }

            if (raioKm > 15)
            {
                raioKm = 15;
            }

            var codigoMunicipio =
                await ObterCodigoMunicipioIbgeAsync(
                    uf,
                    cidade);

            if (!codigoMunicipio.HasValue)
            {
                return new JsonResult(new
                {
                    success = false,

                    message =
                        $"Município {cidade}/{uf} " +
                        "não encontrado no IBGE."
                });
            }

            var empresas =
                await BuscarFarmaciasMinhaReceitaAsync(
                    codigoMunicipio.Value,
                    uf);

            if (empresas.Count == 0)
            {
                return new JsonResult(new
                {
                    success = true,
                    data = Array.Empty<object>(),
                    total = 0,
                    message =
                        "Nenhuma empresa farmacêutica " +
                        "retornada pela base pública."
                });
            }

            var clientes =
                await _context.Clientes
                    .AsNoTracking()
                    .Where(c => c.Cnpj != null)
                    .ToListAsync();

            var leads =
                await _context.Leads
                    .AsNoTracking()
                    .Where(l => l.Cnpj != null)
                    .ToListAsync();

            var cnpjsClientes =
                clientes
                    .Select(c =>
                        NormalizarCnpj(c.Cnpj))
                    .ToHashSet();

            var cnpjsLeads =
                leads
                    .Select(l =>
                        NormalizarCnpj(l.Cnpj))
                    .ToHashSet();

            var pontosOsm =
                await BuscarFarmaciasOsmAsync(
                    latitude,
                    longitude,
                    raioKm);

            var resultado =
                new List<PossivelLeadMapaVm>();

            var usados =
                new HashSet<string>();

            foreach (var ponto in pontosOsm)
            {
                EmpresaPublica? melhor =
                    null;

                var melhorScore =
                    0;

                foreach (var empresa in empresas)
                {
                    var cnpj =
                        NormalizarCnpj(
                            empresa.Cnpj);

                    if (cnpjsClientes.Contains(cnpj) ||
                        cnpjsLeads.Contains(cnpj) ||
                        usados.Contains(cnpj))
                    {
                        continue;
                    }

                    var score =
                        CalcularScore(
                            ponto,
                            empresa);

                    if (score > melhorScore)
                    {
                        melhorScore = score;
                        melhor = empresa;
                    }
                }

                if (melhor == null ||
                    melhorScore < 85)
                {
                    continue;
                }

                var cnpjMelhor =
                    NormalizarCnpj(
                        melhor.Cnpj);

                usados.Add(cnpjMelhor);

                resultado.Add(
                    new PossivelLeadMapaVm
                    {
                        Cnpj =
                            FormatarCnpj(
                                melhor.Cnpj),

                        RazaoSocial =
                            melhor.RazaoSocial,

                        NomeFantasia =
                            string.IsNullOrWhiteSpace(
                                melhor.NomeFantasia)
                                ? melhor.RazaoSocial
                                : melhor.NomeFantasia,

                        Endereco =
                            MontarEndereco(
                                JuntarTipoLogradouro(
                                    melhor.TipoLogradouro,
                                    melhor.Logradouro),
                                melhor.Numero,
                                melhor.Bairro,
                                melhor.Cidade,
                                melhor.Uf,
                                melhor.Cep),

                        Latitude =
                            ponto.Latitude,

                        Longitude =
                            ponto.Longitude,

                        Confianca =
                            melhorScore
                    });
            }

            return new JsonResult(new
            {
                success = true,

                total =
                    resultado.Count,

                data =
                    resultado,

                message =
                    $"{resultado.Count} possíveis leads " +
                    "identificados no raio."
            });
        }

        // =========================================================
        // CAPTURA
        // =========================================================

        public async Task<IActionResult>
            OnPostCapturarLeadAsync(
                [FromBody] CapturarLeadInput input)
        {
            if (input == null ||
                string.IsNullOrWhiteSpace(input.Cnpj))
            {
                return new JsonResult(new
                {
                    success = false,
                    message = "CNPJ inválido."
                });
            }

            var cnpj =
                NormalizarCnpj(input.Cnpj);

            var clientes =
                await _context.Clientes
                    .AsNoTracking()
                    .Where(c => c.Cnpj != null)
                    .ToListAsync();

            if (clientes.Any(c =>
                NormalizarCnpj(c.Cnpj) == cnpj))
            {
                return new JsonResult(new
                {
                    success = false,
                    message =
                        "Esta empresa já é cliente."
                });
            }

            var leads =
                await _context.Leads
                    .AsNoTracking()
                    .Where(l => l.Cnpj != null)
                    .ToListAsync();

            if (leads.Any(l =>
                NormalizarCnpj(l.Cnpj) == cnpj))
            {
                return new JsonResult(new
                {
                    success = false,
                    message =
                        "Este CNPJ já foi capturado."
                });
            }

            var responsavel =
                ObterUsuarioAtual();

            var lead =
                new Lead
                {
                    Nome =
                        string.IsNullOrWhiteSpace(
                            input.NomeFantasia)
                            ? input.RazaoSocial
                            : input.NomeFantasia,

                    Cnpj =
                        input.Cnpj,

                    Endereco =
                        input.Endereco,

                    Latitude =
                        input.Latitude,

                    Longitude =
                        input.Longitude,

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
                    id = lead.Id,
                    nome = lead.Nome,
                    cnpj = lead.Cnpj,
                    endereco = lead.Endereco,
                    latitude = lead.Latitude,
                    longitude = lead.Longitude,
                    status = lead.Status,
                    responsavel =
                        lead.VendedorResponsavel,
                    dataCaptura =
                        lead.DataCriacao.ToString(
                            "dd/MM/yyyy HH:mm")
                }
            });
        }

        // =========================================================
        // STATUS
        // =========================================================

        public async Task<IActionResult>
            OnPostAtualizarStatusAsync(
                [FromBody] AtualizarStatusInput input)
        {
            var permitidos =
                new[]
                {
                    "Em Andamento",
                    "Negociando",
                    "Convertido",
                    "Perdido"
                };

            var status =
                permitidos.FirstOrDefault(s =>
                    string.Equals(
                        s,
                        input.Status,
                        StringComparison.OrdinalIgnoreCase));

            if (status == null)
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
                        l.Id == input.LeadId);

            if (lead == null)
            {
                return new JsonResult(new
                {
                    success = false,
                    message =
                        "Lead não encontrado."
                });
            }

            lead.Status = status;

            await _context.SaveChangesAsync();

            return new JsonResult(new
            {
                success = true,
                status,
                message = "Status atualizado."
            });
        }

        // =========================================================
        // MINHA RECEITA
        // =========================================================

        private async Task<List<EmpresaPublica>>
            BuscarFarmaciasMinhaReceitaAsync(
                int municipioIbge,
                string uf)
        {
            var resultado =
                new Dictionary<string, EmpresaPublica>();

            var cnaes =
                new[]
                {
                    "4771701",
                    "4771702",
                    "4771703"
                };

            foreach (var cnae in cnaes)
            {
                string? cursor = null;
                var pagina = 0;

                do
                {
                    pagina++;

                    if (pagina > 5)
                    {
                        break;
                    }

                    var url =
                        "https://minhareceita.org/" +
                        "?uf=" +
                        Uri.EscapeDataString(uf) +
                        "&municipio=" +
                        municipioIbge +
                        "&cnae=" +
                        cnae +
                        "&limit=1000";

                    if (!string.IsNullOrWhiteSpace(
                        cursor))
                    {
                        url +=
                            "&cursor=" +
                            Uri.EscapeDataString(cursor);
                    }

                    using var response =
                        await Http.GetAsync(url);

                    if (!response.IsSuccessStatusCode)
                    {
                        break;
                    }

                    using var doc =
                        JsonDocument.Parse(
                            await response.Content
                                .ReadAsStringAsync());

                    if (!doc.RootElement
                        .TryGetProperty(
                            "data",
                            out var data))
                    {
                        break;
                    }

                    foreach (var item
                        in data.EnumerateArray())
                    {
                        var empresa =
                            ConverterEmpresa(item);

                        if (empresa == null)
                        {
                            continue;
                        }

                        var cnpj =
                            NormalizarCnpj(
                                empresa.Cnpj);

                        if (string.IsNullOrWhiteSpace(
                            cnpj))
                        {
                            continue;
                        }

                        resultado[cnpj] =
                            empresa;
                    }

                    cursor = null;

                    if (doc.RootElement
                        .TryGetProperty(
                            "cursor",
                            out var cursorJson) &&
                        cursorJson.ValueKind ==
                        JsonValueKind.String)
                    {
                        cursor =
                            cursorJson.GetString();
                    }
                }
                while (!string.IsNullOrWhiteSpace(
                    cursor));
            }

            return resultado.Values.ToList();
        }

        private static EmpresaPublica?
            ConverterEmpresa(
                JsonElement item)
        {
            var cnpj =
                JsonString(item, "cnpj");

            if (string.IsNullOrWhiteSpace(cnpj))
            {
                return null;
            }

            return new EmpresaPublica
            {
                Cnpj = cnpj,

                RazaoSocial =
                    JsonString(
                        item,
                        "razao_social"),

                NomeFantasia =
                    JsonString(
                        item,
                        "nome_fantasia"),

                TipoLogradouro =
                    JsonString(
                        item,
                        "descricao_tipo_de_logradouro"),

                Logradouro =
                    JsonString(
                        item,
                        "logradouro"),

                Numero =
                    JsonString(
                        item,
                        "numero"),

                Bairro =
                    JsonString(
                        item,
                        "bairro"),

                Cidade =
                    JsonString(
                        item,
                        "municipio"),

                Uf =
                    JsonString(
                        item,
                        "uf"),

                Cep =
                    JsonString(
                        item,
                        "cep"),

                Telefone =
                    JsonString(
                        item,
                        "ddd_telefone_1")
            };
        }

        // =========================================================
        // OSM
        // =========================================================

        private async Task<List<PontoOsm>>
            BuscarFarmaciasOsmAsync(
                double latitude,
                double longitude,
                double raioKm)
        {
            var raio =
                Math.Round(raioKm * 1000);

            var lat =
                latitude.ToString(
                    CultureInfo.InvariantCulture);

            var lon =
                longitude.ToString(
                    CultureInfo.InvariantCulture);

            var query = $@"
                [out:json][timeout:30];

                (
                    node[""amenity""=""pharmacy""](around:{raio},{lat},{lon});
                    way[""amenity""=""pharmacy""](around:{raio},{lat},{lon});
                    relation[""amenity""=""pharmacy""](around:{raio},{lat},{lon});
                );

                out center tags;
            ";

            var endpoints =
                new[]
                {
                    "https://overpass-api.de/api/interpreter",
                    "https://overpass.kumi.systems/api/interpreter"
                };

            foreach (var endpoint in endpoints)
            {
                try
                {
                    using var content =
                        new StringContent(
                            "data=" +
                            Uri.EscapeDataString(query),
                            Encoding.UTF8,
                            "application/x-www-form-urlencoded");

                    using var response =
                        await Http.PostAsync(
                            endpoint,
                            content);

                    if (!response.IsSuccessStatusCode)
                    {
                        continue;
                    }

                    using var doc =
                        JsonDocument.Parse(
                            await response.Content
                                .ReadAsStringAsync());

                    var resultado =
                        new List<PontoOsm>();

                    foreach (var item
                        in doc.RootElement
                            .GetProperty("elements")
                            .EnumerateArray())
                    {
                        var ponto =
                            ConverterPontoOsm(item);

                        if (ponto != null)
                        {
                            resultado.Add(ponto);
                        }
                    }

                    return resultado;
                }
                catch
                {
                }
            }

            return new List<PontoOsm>();
        }

        private static PontoOsm?
            ConverterPontoOsm(
                JsonElement item)
        {
            double? lat = null;
            double? lon = null;

            if (item.TryGetProperty(
                "lat",
                out var latJson) &&
                latJson.TryGetDouble(out var l1))
            {
                lat = l1;
            }

            if (item.TryGetProperty(
                "lon",
                out var lonJson) &&
                lonJson.TryGetDouble(out var l2))
            {
                lon = l2;
            }

            if ((!lat.HasValue ||
                 !lon.HasValue) &&
                item.TryGetProperty(
                    "center",
                    out var center))
            {
                if (center.TryGetProperty(
                    "lat",
                    out var clat) &&
                    clat.TryGetDouble(out var l3))
                {
                    lat = l3;
                }

                if (center.TryGetProperty(
                    "lon",
                    out var clon) &&
                    clon.TryGetDouble(out var l4))
                {
                    lon = l4;
                }
            }

            if (!lat.HasValue ||
                !lon.HasValue)
            {
                return null;
            }

            JsonElement tags = default;

            item.TryGetProperty(
                "tags",
                out tags);

            return new PontoOsm
            {
                Latitude = lat.Value,
                Longitude = lon.Value,

                Nome =
                    PrimeiroValor(
                        tags,
                        "name",
                        "brand",
                        "operator"),

                Rua =
                    JsonString(
                        tags,
                        "addr:street"),

                Numero =
                    JsonString(
                        tags,
                        "addr:housenumber"),

                Bairro =
                    PrimeiroValor(
                        tags,
                        "addr:suburb",
                        "addr:neighbourhood"),

                Cep =
                    JsonString(
                        tags,
                        "addr:postcode"),

                Cnpj =
                    PrimeiroValor(
                        tags,
                        "cnpj",
                        "contact:cnpj",
                        "ref:cnpj"),

                Telefone =
                    PrimeiroValor(
                        tags,
                        "phone",
                        "contact:phone")
            };
        }

        // =========================================================
        // CORRESPONDÊNCIA
        // =========================================================

        private static int CalcularScore(
            PontoOsm ponto,
            EmpresaPublica empresa)
        {
            var cnpjPonto =
                NormalizarCnpj(
                    ponto.Cnpj);

            var cnpjEmpresa =
                NormalizarCnpj(
                    empresa.Cnpj);

            if (!string.IsNullOrWhiteSpace(
                    cnpjPonto) &&
                cnpjPonto == cnpjEmpresa)
            {
                return 100;
            }

            var telefonePonto =
                SomenteNumeros(
                    ponto.Telefone);

            var telefoneEmpresa =
                SomenteNumeros(
                    empresa.Telefone);

            if (telefonePonto.Length >= 8 &&
                telefoneEmpresa.Length >= 8 &&
                telefonePonto.EndsWith(
                    telefoneEmpresa))
            {
                return 99;
            }

            var nomeEmpresa =
                string.IsNullOrWhiteSpace(
                    empresa.NomeFantasia)
                    ? empresa.RazaoSocial
                    : empresa.NomeFantasia;

            var nome =
                SimilaridadeTokens(
                    ponto.Nome,
                    nomeEmpresa);

            var rua =
                SimilaridadeTokens(
                    ponto.Rua,
                    empresa.Logradouro);

            var numeroIgual =
                !string.IsNullOrWhiteSpace(
                    ponto.Numero) &&
                !string.IsNullOrWhiteSpace(
                    empresa.Numero) &&
                NormalizarNumero(
                    ponto.Numero) ==
                NormalizarNumero(
                    empresa.Numero);

            var cepIgual =
                SomenteNumeros(ponto.Cep) ==
                SomenteNumeros(empresa.Cep) &&
                !string.IsNullOrWhiteSpace(
                    ponto.Cep);

            if (numeroIgual &&
                rua >= 0.70)
            {
                return 98;
            }

            if (cepIgual &&
                nome >= 0.60)
            {
                return 95;
            }

            if (nome >= 0.85 &&
                rua >= 0.55)
            {
                return 90;
            }

            return 0;
        }

        // =========================================================
        // IBGE
        // =========================================================

        private async Task<int?>
            ObterCodigoMunicipioIbgeAsync(
                string uf,
                string cidade)
        {
            try
            {
                using var response =
                    await Http.GetAsync(
                        "https://servicodados.ibge.gov.br/" +
                        "api/v1/localidades/estados/" +
                        Uri.EscapeDataString(uf) +
                        "/municipios");

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                using var doc =
                    JsonDocument.Parse(
                        await response.Content
                            .ReadAsStringAsync());

                var procurar =
                    Normalizar(cidade);

                foreach (var item
                    in doc.RootElement
                        .EnumerateArray())
                {
                    if (Normalizar(
                        JsonString(item, "nome")) !=
                        procurar)
                    {
                        continue;
                    }

                    if (item.GetProperty("id")
                        .TryGetInt32(
                            out var codigo))
                    {
                        return codigo;
                    }
                }
            }
            catch
            {
            }

            return null;
        }

        // =========================================================
        // NOMINATIM
        // =========================================================

        private static async Task<JsonDocument?>
            ConsultarNominatimAsync(
                string url)
        {
            await NominatimLock.WaitAsync();

            try
            {
                var decorrido =
                    DateTime.UtcNow -
                    UltimaChamadaNominatim;

                if (decorrido <
                    TimeSpan.FromSeconds(1))
                {
                    await Task.Delay(
                        TimeSpan.FromSeconds(1) -
                        decorrido);
                }

                using var request =
                    new HttpRequestMessage(
                        HttpMethod.Get,
                        url);

                request.Headers.TryAddWithoutValidation(
                    "User-Agent",
                    "GeoPharma/1.0");

                using var response =
                    await Http.SendAsync(request);

                UltimaChamadaNominatim =
                    DateTime.UtcNow;

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                return JsonDocument.Parse(
                    await response.Content
                        .ReadAsStringAsync());
            }
            finally
            {
                NominatimLock.Release();
            }
        }

        // =========================================================
        // AUXILIARES
        // =========================================================

        private string ObterUsuarioAtual()
        {
            var nome =
                User.Identity?.Name;

            return string.IsNullOrWhiteSpace(nome)
                ? "Admin"
                : nome;
        }

        private static string ObterNomeCliente(
            Cliente c)
        {
            if (!string.IsNullOrWhiteSpace(
                c.NomeFantasia))
            {
                return c.NomeFantasia;
            }

            return c.RazaoSocial ??
                   $"Cliente #{c.Id}";
        }

        private static string MontarEndereco(
            string? logradouro,
            string? numero,
            string? bairro,
            string? cidade,
            string? uf,
            string? cep)
        {
            var partes =
                new List<string>();

            var linha =
                (logradouro ?? "").Trim();

            if (!string.IsNullOrWhiteSpace(
                numero))
            {
                linha +=
                    $", {numero}";
            }

            if (!string.IsNullOrWhiteSpace(
                linha))
            {
                partes.Add(linha);
            }

            if (!string.IsNullOrWhiteSpace(
                bairro))
            {
                partes.Add(bairro);
            }

            if (!string.IsNullOrWhiteSpace(
                cidade))
            {
                partes.Add(
                    string.IsNullOrWhiteSpace(uf)
                        ? cidade
                        : $"{cidade} - {uf}");
            }

            if (!string.IsNullOrWhiteSpace(
                cep))
            {
                partes.Add(
                    $"CEP {cep}");
            }

            return string.Join(
                " | ",
                partes);
        }

        private static string JuntarTipoLogradouro(
            string tipo,
            string logradouro)
        {
            return $"{tipo} {logradouro}"
                .Trim();
        }

        private static string ExtrairCidade(
            JsonElement address)
        {
            return PrimeiroValor(
                address,
                "city",
                "town",
                "municipality",
                "village");
        }

        private static string ExtrairUf(
            JsonElement address)
        {
            var iso =
                PrimeiroValor(
                    address,
                    "ISO3166-2-lvl4");

            if (!string.IsNullOrWhiteSpace(
                iso) &&
                iso.Contains("-"))
            {
                return iso.Split('-')[^1];
            }

            return EstadoParaUf(
                JsonString(
                    address,
                    "state"));
        }

        private static string EstadoParaUf(
            string estado)
        {
            var mapa =
                new Dictionary<string, string>
                {
                    ["RIO DE JANEIRO"] = "RJ",
                    ["SAO PAULO"] = "SP",
                    ["MINAS GERAIS"] = "MG",
                    ["ESPIRITO SANTO"] = "ES",
                    ["PARANA"] = "PR",
                    ["SANTA CATARINA"] = "SC",
                    ["RIO GRANDE DO SUL"] = "RS",
                    ["BAHIA"] = "BA",
                    ["PERNAMBUCO"] = "PE",
                    ["CEARA"] = "CE",
                    ["GOIAS"] = "GO",
                    ["DISTRITO FEDERAL"] = "DF"
                };

            var n =
                Normalizar(estado);

            return mapa.TryGetValue(
                n,
                out var uf)
                ? uf
                : "";
        }

        private static string JsonString(
            JsonElement element,
            string nome)
        {
            if (element.ValueKind !=
                JsonValueKind.Object)
            {
                return "";
            }

            if (!element.TryGetProperty(
                nome,
                out var value))
            {
                return "";
            }

            return value.ValueKind ==
                JsonValueKind.Null
                ? ""
                : value.ToString();
        }

        private static string PrimeiroValor(
            JsonElement element,
            params string[] campos)
        {
            foreach (var campo in campos)
            {
                var valor =
                    JsonString(
                        element,
                        campo);

                if (!string.IsNullOrWhiteSpace(
                    valor))
                {
                    return valor;
                }
            }

            return "";
        }

        private static double? ParseDouble(
            string valor)
        {
            if (double.TryParse(
                valor,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out var numero))
            {
                return numero;
            }

            return null;
        }

        private static string NormalizarCnpj(
            string? cnpj)
        {
            return new string(
                (cnpj ?? "")
                    .Where(char.IsLetterOrDigit)
                    .ToArray());
        }

        private static string FormatarCnpj(
            string? cnpj)
        {
            var n =
                NormalizarCnpj(cnpj);

            if (n.Length == 14 &&
                n.All(char.IsDigit))
            {
                return
                    $"{n[..2]}." +
                    $"{n.Substring(2, 3)}." +
                    $"{n.Substring(5, 3)}/" +
                    $"{n.Substring(8, 4)}-" +
                    $"{n.Substring(12, 2)}";
            }

            return n;
        }

        private static string SomenteNumeros(
            string? valor)
        {
            return new string(
                (valor ?? "")
                    .Where(char.IsDigit)
                    .ToArray());
        }

        private static string NormalizarNumero(
            string? numero)
        {
            return new string(
                (numero ?? "")
                    .Where(char.IsLetterOrDigit)
                    .ToArray());
        }

        private static string Normalizar(
            string? texto)
        {
            if (string.IsNullOrWhiteSpace(
                texto))
            {
                return "";
            }

            var value =
                texto
                    .ToUpperInvariant()
                    .Normalize(
                        NormalizationForm.FormD);

            return new string(
                value.Where(c =>
                    CharUnicodeInfo
                        .GetUnicodeCategory(c) !=
                    UnicodeCategory.NonSpacingMark)
                    .ToArray());
        }

        private static double SimilaridadeTokens(
            string? a,
            string? b)
        {
            var ta =
                Tokenizar(a);

            var tb =
                Tokenizar(b);

            if (ta.Count == 0 ||
                tb.Count == 0)
            {
                return 0;
            }

            var intersecao =
                ta.Intersect(tb).Count();

            var uniao =
                ta.Union(tb).Count();

            return uniao == 0
                ? 0
                : (double)intersecao / uniao;
        }

        private static HashSet<string> Tokenizar(
            string? texto)
        {
            return Normalizar(texto)
                .Split(
                    new[]
                    {
                        ' ',
                        '-',
                        '.',
                        ',',
                        '/'
                    },
                    StringSplitOptions
                        .RemoveEmptyEntries)
                .Where(x => x.Length > 1)
                .ToHashSet();
        }

        private static HttpClient CriarHttpClient()
        {
            var client =
                new HttpClient
                {
                    Timeout =
                        TimeSpan.FromSeconds(45)
                };

            client.DefaultRequestHeaders
                .TryAddWithoutValidation(
                    "User-Agent",
                    "GeoPharma/1.0");

            return client;
        }
    }

    public class ClienteMapaVm
    {
        public int Id { get; set; }
        public string Nome { get; set; } = "";
        public string RazaoSocial { get; set; } = "";
        public string Cnpj { get; set; } = "";
        public string Endereco { get; set; } = "";
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public class LeadMapaVm
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

    public class EnderecoPesquisaVm
    {
        public string Endereco { get; set; } = "";
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Cidade { get; set; } = "";
        public string Uf { get; set; } = "";
    }

    public class CapturarLeadInput
    {
        public string Cnpj { get; set; } = "";
        public string RazaoSocial { get; set; } = "";
        public string NomeFantasia { get; set; } = "";
        public string Endereco { get; set; } = "";
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public class AtualizarStatusInput
    {
        public int LeadId { get; set; }
        public string Status { get; set; } = "";
    }

    internal class EmpresaPublica
    {
        public string Cnpj { get; set; } = "";
        public string RazaoSocial { get; set; } = "";
        public string NomeFantasia { get; set; } = "";
        public string TipoLogradouro { get; set; } = "";
        public string Logradouro { get; set; } = "";
        public string Numero { get; set; } = "";
        public string Bairro { get; set; } = "";
        public string Cidade { get; set; } = "";
        public string Uf { get; set; } = "";
        public string Cep { get; set; } = "";
        public string Telefone { get; set; } = "";
    }

    internal class PontoOsm
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Nome { get; set; } = "";
        public string Rua { get; set; } = "";
        public string Numero { get; set; } = "";
        public string Bairro { get; set; } = "";
        public string Cep { get; set; } = "";
        public string Cnpj { get; set; } = "";
        public string Telefone { get; set; } = "";
    }

    public class PossivelLeadMapaVm
    {
        public string Cnpj { get; set; } = "";
        public string RazaoSocial { get; set; } = "";
        public string NomeFantasia { get; set; } = "";
        public string Endereco { get; set; } = "";
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int Confianca { get; set; }
    }
}