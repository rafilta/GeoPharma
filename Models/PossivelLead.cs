using System.ComponentModel.DataAnnotations;

namespace GeoPharma.Models
{
    public class PossivelLead
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(14)]
        public string Cnpj { get; set; } = string.Empty;

        [MaxLength(300)]
        public string RazaoSocial { get; set; } = string.Empty;

        [MaxLength(300)]
        public string NomeFantasia { get; set; } = string.Empty;

        public int Cnae { get; set; }

        [MaxLength(20)]
        public string Cep { get; set; } = string.Empty;

        [MaxLength(50)]
        public string TipoLogradouro { get; set; } = string.Empty;

        [MaxLength(300)]
        public string Logradouro { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Numero { get; set; } = string.Empty;

        [MaxLength(300)]
        public string Complemento { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Bairro { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Cidade { get; set; } = string.Empty;

        [MaxLength(2)]
        public string Uf { get; set; } = string.Empty;

        public int? CodigoMunicipioIbge { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        [MaxLength(50)]
        public string PrecisaoGeografica { get; set; } = string.Empty;

        public bool EnderecoValidado { get; set; }

        [MaxLength(50)]
        public string SituacaoCadastral { get; set; } = string.Empty;

        public DateTime AtualizadoEm { get; set; } = DateTime.UtcNow;
    }
}