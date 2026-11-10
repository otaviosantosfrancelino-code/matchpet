using System.ComponentModel.DataAnnotations;

namespace crudcomdb.Models
{
    // =====================================================================
    // ACHADOS E PERDIDOS: anúncio de pet perdido ou de pet encontrado na rua
    // =====================================================================
    public class AnimalPerdido
    {
        public const string TipoPerdido = "Perdido";       // o dono perdeu o pet e procura por ele
        public const string TipoEncontrado = "Encontrado"; // alguém encontrou um pet que parece ter dono

        public const string StatusAtivo = "Ativo";          // aparece no catálogo
        public const string StatusResolvido = "Resolvido";  // pet reencontrou a família (aparece como ENCONTRADO)

        [Key] public int Id { get; set; }
        public string Tipo { get; set; } = TipoPerdido;

        // Quem encontra um pet quase nunca sabe o nome: por isso é opcional
        [MaxLength(60)] public string? Nome { get; set; }

        public string Especie { get; set; } = "Cão";
        [MaxLength(60)] public string? Raca { get; set; }
        public string Porte { get; set; } = "Médio";
        public string Sexo { get; set; } = "Não sei";
        [MaxLength(40)] public string Cor { get; set; } = "";
        public bool UsaColeira { get; set; }
        [MaxLength(300)] public string? SinaisParticulares { get; set; } // manchas, cicatriz, coleira, mancando...
        [MaxLength(2000)] public string Descricao { get; set; } = "";

        // Onde e quando foi perdido/visto (só bairro e cidade, nunca endereço completo)
        [MaxLength(80)] public string Bairro { get; set; } = "";
        [MaxLength(80)] public string Cidade { get; set; } = "Franca";
        public string Estado { get; set; } = "SP";
        public DateTime DataOcorrido { get; set; } = DateTime.UtcNow;

        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        public string Status { get; set; } = "Em Moderação"; // Em Moderação, Ativo, Resolvido, Removido
        public DateTime DataCadastro { get; set; } = DateTime.UtcNow;
        public DateTime? DataResolucao { get; set; }
        public DateTime? UltimoLembrete { get; set; } // lembrete "ele já foi encontrado?" a cada 2 dias

        public List<AnimalPerdidoImagem> Imagens { get; set; } = new();

        // Título usado quando não há nome: "Cão perdido", "Gata encontrada"...
        public string Titulo => !string.IsNullOrWhiteSpace(Nome) ? Nome!
            : $"{EspecieNoGenero} {(Tipo == TipoPerdido ? (Feminino ? "perdida" : "perdido") : (Feminino ? "encontrada" : "encontrado"))}";

        private bool Feminino => Sexo == "Fêmea";
        private string EspecieNoGenero => (Especie, Feminino) switch
        {
            ("Cão", true) => "Cadela",
            ("Gato", true) => "Gata",
            ("Pássaro", _) => "Ave",
            _ => Especie
        };
    }

    public class AnimalPerdidoImagem
    {
        [Key] public int Id { get; set; }
        public int AnimalPerdidoId { get; set; }
        public string Base64Data { get; set; } = "";
    }
}
