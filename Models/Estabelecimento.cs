namespace GeoPharma.Models
{
    public class Estabelecimento
    {
        public int Id { get; set; }
        public string? Regiao { get; set; }
        public string? NomeFantasia { get; set; }
        public string? RazaoSocial { get; set; }
        public string? Cnpj { get; set; }
        public string? Cep { get; set; }
        public string? Logradouro { get; set; }
        public string? Numero { get; set; }
        public string? Bairro { get; set; }
        public string? Cidade { get; set; }
        public string? Uf { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public bool Ativo { get; set; } = true;
    }
}