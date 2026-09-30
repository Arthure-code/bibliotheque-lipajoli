using BibliothequeLIPAJOLI.Models;

namespace BibliothequeLIPAJOLI
{
    public class Edition
    {
        public int EditionID { get; set; }

        public string? NomEditeur { get; set; }

        public DateTime? AnneeEdition { get; set; }

        public int LivreID { get; set; }
        public Livre? Livre { get; set; }

        public ICollection<Exemplaire> Exemplaires { get; set; } = new List<Exemplaire>();
    }
}
