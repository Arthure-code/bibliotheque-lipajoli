using System.ComponentModel.DataAnnotations;

namespace BibliothequeLIPAJOLI.Models
{
    public class Adresse
    {
        public int AdresseID { get; set; }
       
        public string? Rue { get; set; }

        public string? Ville { get; set; }

        public string? Province { get; set; }

        public string? CodePostale { get; set; }

        public ICollection<UsagerAdresse> UsagerAdresses { get; set; } = new List<UsagerAdresse>();
    }
}
