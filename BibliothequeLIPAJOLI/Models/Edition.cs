using BibliothequeLIPAJOLI.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BibliothequeLIPAJOLI
{
    public class Edition
    {
        [BindNever]
        public int EditionID { get; set; }

        public string? NomEditeur { get; set; }

        public DateTime? AnneeEdition { get; set; }

        [BindNever]
        public int LivreID { get; set; }
        public Livre? Livre { get; set; }

        public ICollection<Exemplaire> Exemplaires { get; set; } = new List<Exemplaire>();
    }
}
