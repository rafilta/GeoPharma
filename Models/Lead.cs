namespace GeoPharma.Models
{
    public class Lead
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Endereco { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? Cnpj { get; set; }

        // Propriedades ajustadas para bater com a tela Index.cshtml
        public string Status { get; set; } = "Em Andamento";
        public string VendedorResponsavel { get; set; } = string.Empty;
        public DateTime DataCriacao { get; set; } = DateTime.Now;
    }
}