using BibliothequeLIPAJOLI.Data;
using BibliothequeLIPAJOLI.Interfaces;
using BibliothequeLIPAJOLI.Models;
using BibliothequeLIPAJOLI.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BibliothequeLIPAJOLI.Services
{
    public class GestionUsager : IUsagerService
    {
        private readonly BibliothequeLipajoliContext _context;

        public GestionUsager(BibliothequeLipajoliContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Usager>> ObtenirTousLesUsagersAsync()
        {
            return await _context.Usagers
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Usager?> ObtenirUsagerParIdAsync(int identifiantUsager)
        {
            return await _context.Usagers
                .Include(u => u.Emprunts)
                .Include(u => u.UsagerAdresses)
                    .ThenInclude(ua => ua.Adresse)
                .AsSplitQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.ID == identifiantUsager);
        }

        public async Task<IEnumerable<Usager>> RechercherUsagersAsync(string texteCherche)
        {
            return await _context.Usagers
                .Where(u => u.Nom.Contains(texteCherche) || u.Prenom.Contains(texteCherche))
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task SupprimerUsagerAsync(int identifiantUsager)
        {
            var usager = await _context.Usagers.FindAsync(identifiantUsager);
            if (usager != null)
            {
                _context.Usagers.Remove(usager);
                await _context.SaveChangesAsync();
            }
        }

        public async Task AjouterUsagerCompletAsync(UsagerCreateViewModel formulaire)
        {
            var usagerAdresse = new UsagerAdresse
            {
                Usager = formulaire.Usager,
                Adresse = formulaire.Adresse,
                TypeAdresse = formulaire.TypeAdresse
            };

            _context.UsagerAdresses.Add(usagerAdresse);
            await _context.SaveChangesAsync();
        }

        public async Task MettreAJourUsagerCompletAsync(int identifiantUsager, UsagerCreateViewModel formulaire)
        {
            var usager = await _context.Usagers.FindAsync(identifiantUsager);
            if (usager == null)
            {
                return;
            }

            usager.Nom = formulaire.Usager.Nom;
            usager.Prenom = formulaire.Usager.Prenom;
            usager.Courriel = formulaire.Usager.Courriel;
            usager.Statut = formulaire.Usager.Statut;
            usager.NumeroAbonne = formulaire.Usager.NumeroAbonne;

            var usagerAdresse = await _context.UsagerAdresses
                .Include(ua => ua.Adresse)
                .FirstOrDefaultAsync(ua => ua.UsagerID == identifiantUsager
                    && ua.TypeAdresse == formulaire.TypeAdresse);

            if (usagerAdresse?.Adresse != null)
            {
                usagerAdresse.Adresse.Rue = formulaire.Adresse.Rue;
                usagerAdresse.Adresse.Ville = formulaire.Adresse.Ville;
                usagerAdresse.Adresse.Province = formulaire.Adresse.Province;
                usagerAdresse.Adresse.CodePostale = formulaire.Adresse.CodePostale;
            }
            else
            {
                _context.UsagerAdresses.Add(new UsagerAdresse
                {
                    UsagerID = identifiantUsager,
                    Adresse = formulaire.Adresse,
                    TypeAdresse = formulaire.TypeAdresse
                });
            }

            await _context.SaveChangesAsync();
        }
    }
}
