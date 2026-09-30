using BibliothequeLIPAJOLI.Models;

namespace BibliothequeLIPAJOLI.Interfaces
{
    // Le dossier des usagers vu par le service : des usagers et leurs
    // adresses, jamais une requete.
    public interface IUsagerDepot
    {
        Task<List<Usager>> ListerAsync();

        Task<Usager?> ObtenirAvecSesLiensAsync(int identifiantUsager);

        Task<Usager?> ObtenirAsync(int identifiantUsager);

        Task<List<Usager>> ChercherAsync(string texteCherche);

        Task<UsagerAdresse?> ObtenirLAdresseAsync(int identifiantUsager, TypeAdresse? typeAdresse);

        void AjouterUneAdresse(UsagerAdresse lien);

        void Supprimer(Usager usager);

        Task EnregistrerAsync();
    }
}
