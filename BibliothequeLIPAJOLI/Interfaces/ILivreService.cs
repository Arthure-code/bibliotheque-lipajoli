using BibliothequeLIPAJOLI.Models;
using BibliothequeLIPAJOLI.ViewModels;

namespace BibliothequeLIPAJOLI.Interfaces
{
    public interface ILivreService
    {
        Task<Livre?> ObtenirLivreParIdAsync(int identifiantLivre);

        Task<List<Livre>> RechercherLivresAsync(string? texteCherche = null, int? categorieId = null);

        List<Livre> TrierLivres(List<Livre> livres, string ordre);

        Task<Livre> CreerLivreCompletAsync(LivreCreateViewModel formulaire);

        Task<Livre?> ModifierLivreCompletAsync(LivreEditViewModel formulaire);

        Task<bool> SupprimerLivreAsync(int identifiantLivre);
    }
}
