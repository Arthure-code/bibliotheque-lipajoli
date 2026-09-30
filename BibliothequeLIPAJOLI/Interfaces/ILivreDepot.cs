using BibliothequeLIPAJOLI.Models;

namespace BibliothequeLIPAJOLI.Interfaces
{
    // Tout ce que le catalogue demande a la base, et rien de plus : le
    // service qui s'en sert ne sait pas qu'il y a une base derriere.
    public interface ILivreDepot
    {
        Task<Livre?> ObtenirAvecSesLiensAsync(int identifiantLivre);

        Task<Livre?> ObtenirPourModificationAsync(int identifiantLivre);

        Task<Livre?> ObtenirAsync(int identifiantLivre);

        Task<List<Livre>> ChercherAsync(string? titreCherche, IReadOnlyCollection<int> auteurs, int? categorieId);

        Task<List<string?>> ListerLesCodesCommencantParAsync(string prefixe);

        void Ajouter(Livre livre);

        void RemplacerLesValeurs(Livre livre, Livre valeurs);

        void Supprimer(Livre livre);

        Task EnregistrerAsync();
    }
}
