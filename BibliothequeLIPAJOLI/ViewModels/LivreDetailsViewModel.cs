using BibliothequeLIPAJOLI.Models;

namespace BibliothequeLIPAJOLI.ViewModels
{
    public class LivreDetailsViewModel
    {
        public Livre Livre { get; set; } = new Livre();

        public List<Exemplaire> Exemplaires { get; set; } = new List<Exemplaire>();

        public List<Emprunt> Emprunts { get; set; } = new List<Emprunt>();
    }
}
