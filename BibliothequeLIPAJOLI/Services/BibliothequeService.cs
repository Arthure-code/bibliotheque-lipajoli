using BibliothequeLIPAJOLI.Interfaces;
using BibliothequeLIPAJOLI.Models;

namespace BibliothequeLIPAJOLI.Services
{
    public class BibliothequeService : IReferentielService
    {
        private const int JoursDePretParDefaut = 14;

        private readonly List<Categorie> _categories;
        private readonly List<Auteur> _auteurs;

        public int JoursDePret { get; }

        public BibliothequeService(IConfiguration configuration)
        {
            var section = configuration.GetSection("Bibliotheque");

            _categories = section.GetSection("Categories").Get<List<Categorie>>() ?? new List<Categorie>();
            _auteurs = section.GetSection("Auteurs").Get<List<Auteur>>() ?? new List<Auteur>();

            int joursLus = section.GetValue<int>("JoursDePret");
            JoursDePret = joursLus > 0 ? joursLus : JoursDePretParDefaut;
        }
        public Categorie? ObtenirCategorieParId(int identifiantCategorie)
        {
            return _categories.FirstOrDefault(c => c.CategorieID == identifiantCategorie);
        }

        public List<Categorie> ObtenirCategories()
        {
            return _categories.ToList();
        }
        public Auteur? ObtenirAuteurParId(int identifiantAuteur)
        {
            return _auteurs.FirstOrDefault(a => a.ID == identifiantAuteur);
        }

        public List<Auteur> ObtenirAuteurs()
        {
            return _auteurs.ToList();
        }
    }
}
