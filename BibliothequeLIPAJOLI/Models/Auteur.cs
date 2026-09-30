using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BibliothequeLIPAJOLI.Models
{
  
    public class Auteur : Personne
    {
        public DateTime? DateDeces { get; set; }
        public string? Biographie { get; set; } = string.Empty;

        public string? PhotoUrl { get; set; } = string.Empty;

        public ICollection<Redaction> Redactions { get; set; } = new List<Redaction>();
    }
}
