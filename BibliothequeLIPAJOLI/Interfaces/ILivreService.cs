using BibliothequeLIPAJOLI.Models;
using BibliothequeLIPAJOLI.ViewModels;

namespace BibliothequeLIPAJOLI.Interfaces
{
    public interface ILivreService
    {
        Task<Livre?> ObtenirLivreParIdAsync(int identifiantLivre);

        Task<List<Livre>> RechercherLivresAsync(string? texteCherche = null, int? categorieId = null);

        List<Livre> TrierLivres(List<Livre> livres, string ordre);

        Task<Livre> CreerLivreCompletAsync(LivreFormulaireViewModel formulaire);

        Task<Livre?> ModifierLivreCompletAsync(int identifiantLivre, LivreFormulaireViewModel formulaire);

        Task<bool> SupprimerLivreAsync(int identifiantLivre);
    }
}
