using BibliothequeLIPAJOLI.Models;
using BibliothequeLIPAJOLI.ViewModels;

namespace BibliothequeLIPAJOLI.Interfaces
{
    public interface IUsagerService
    {
        Task<IEnumerable<Usager>> ObtenirTousLesUsagersAsync();

        Task<Usager?> ObtenirUsagerParIdAsync(int identifiantUsager);

        Task<IEnumerable<Usager>> RechercherUsagersAsync(string texteCherche);

        Task AjouterUsagerCompletAsync(UsagerCreateViewModel formulaire);

        Task MettreAJourUsagerCompletAsync(int identifiantUsager, UsagerCreateViewModel formulaire);

        Task SupprimerUsagerAsync(int identifiantUsager);
    }
}
