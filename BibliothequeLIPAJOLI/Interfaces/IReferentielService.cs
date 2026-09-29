using BibliothequeLIPAJOLI.Models;

namespace BibliothequeLIPAJOLI.Interfaces
{
    public interface IReferentielService
    {
        int JoursDePret { get; }

        List<Categorie> ObtenirCategories();

        Categorie? ObtenirCategorieParId(int identifiantCategorie);

        List<Auteur> ObtenirAuteurs();

        Auteur? ObtenirAuteurParId(int identifiantAuteur);
    }
}
