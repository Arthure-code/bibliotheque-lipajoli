using BibliothequeLIPAJOLI.Interfaces;
using BibliothequeLIPAJOLI.Models;
using BibliothequeLIPAJOLI.ViewModels;

namespace BibliothequeLIPAJOLI.Services
{
    public class GestionUsager : IUsagerService
    {
        private readonly IUsagerDepot _depot;

        public GestionUsager(IUsagerDepot depot)
        {
            _depot = depot;
        }

        public async Task<IEnumerable<Usager>> ObtenirTousLesUsagersAsync()
        {
            return await _depot.ListerAsync();
        }

        public async Task<Usager?> ObtenirUsagerParIdAsync(int identifiantUsager)
        {
            return await _depot.ObtenirAvecSesLiensAsync(identifiantUsager);
        }

        public async Task<IEnumerable<Usager>> RechercherUsagersAsync(string texteCherche)
        {
            return await _depot.ChercherAsync(texteCherche);
        }

        public async Task SupprimerUsagerAsync(int identifiantUsager)
        {
            Usager? usager = await _depot.ObtenirAsync(identifiantUsager);
            if (usager == null)
            {
                return;
            }

            _depot.Supprimer(usager);
            await _depot.EnregistrerAsync();
        }

        public async Task AjouterUsagerCompletAsync(UsagerCreateViewModel formulaire)
        {
            ArgumentNullException.ThrowIfNull(formulaire);

            // L usager, son adresse et le lien entre les deux partent
            // ensemble : un echec a mi-chemin ne laisse pas d adresse seule.
            _depot.AjouterUneAdresse(new UsagerAdresse
            {
                Usager = formulaire.Usager,
                Adresse = formulaire.Adresse,
                TypeAdresse = formulaire.TypeAdresse
            });

            await _depot.EnregistrerAsync();
        }

        public async Task MettreAJourUsagerCompletAsync(int identifiantUsager, UsagerCreateViewModel formulaire)
        {
            ArgumentNullException.ThrowIfNull(formulaire);

            Usager? usager = await _depot.ObtenirAsync(identifiantUsager);
            if (usager == null)
            {
                return;
            }

            usager.Nom = formulaire.Usager.Nom;
            usager.Prenom = formulaire.Usager.Prenom;
            usager.Courriel = formulaire.Usager.Courriel;
            usager.Statut = formulaire.Usager.Statut;
            usager.NumeroAbonne = formulaire.Usager.NumeroAbonne;

            UsagerAdresse? lien = await _depot.ObtenirLAdresseAsync(identifiantUsager, formulaire.TypeAdresse);

            if (lien?.Adresse != null)
            {
                lien.Adresse.Rue = formulaire.Adresse.Rue;
                lien.Adresse.Ville = formulaire.Adresse.Ville;
                lien.Adresse.Province = formulaire.Adresse.Province;
                lien.Adresse.CodePostale = formulaire.Adresse.CodePostale;
            }
            else
            {
                _depot.AjouterUneAdresse(new UsagerAdresse
                {
                    UsagerID = identifiantUsager,
                    Adresse = formulaire.Adresse,
                    TypeAdresse = formulaire.TypeAdresse
                });
            }

            await _depot.EnregistrerAsync();
        }
    }
}
