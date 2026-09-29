using System.ComponentModel.DataAnnotations.Schema;

namespace BibliothequeLIPAJOLI.Models
{
    public class Redaction
    {
        public string? Role { get; set; }
        public int? Ordre { get; set; } = 1;
        public int LivreID { get; set; }
        public Livre? Livre { get; set; }

        public int AuteurID { get; set; }

        [NotMapped]
        public Auteur? Auteur { get; set; }
    }
}
