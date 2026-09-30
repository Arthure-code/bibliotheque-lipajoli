using BibliothequeLIPAJOLI.Data;
using BibliothequeLIPAJOLI.Interfaces;
using BibliothequeLIPAJOLI.Models;
using BibliothequeLIPAJOLI.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BibliothequeLIPAJOLI.Services
{
    public class GestionUsager : IUsagerService
    {
        private readonly BibliothequeLIPAJOLIContext _context;

        public GestionUsager(BibliothequeLIPAJOLIContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Usager>> ObtenirTousLesUsagersAsync()
        {
            return await _context.Usagers
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Usager?> ObtenirUsagerParIdAsync(int id)
        {
            return await _context.Usagers
                .Include(u => u.Emprunts)
                .Include(u => u.UsagerAdresses)
                    .ThenInclude(ua => ua.Adresse)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.ID == id);
        }

        public async Task<IEnumerable<Usager>> RechercherUsagersAsync(string chaineRecherche)
        {
            return await _context.Usagers
                .Where(u => u.Nom.Contains(chaineRecherche) || u.Prenom.Contains(chaineRecherche))
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task SupprimerUsagerAsync(int id)
        {
            var usager = await _context.Usagers.FindAsync(id);
            if (usager != null)
            {
                _context.Usagers.Remove(usager);
                await _context.SaveChangesAsync();
            }
        }

        public async Task AjouterUsagerCompletAsync(UsagerCreateViewModel model)
        {
            var usagerAdresse = new UsagerAdresse
            {
                Usager = model.Usager,
                Adresse = model.Adresse,
                TypeAdresse = model.TypeAdresse
            };

            _context.UsagerAdresses.Add(usagerAdresse);
            await _context.SaveChangesAsync();
        }

        public async Task MettreAJourUsagerCompletAsync(int id, UsagerCreateViewModel model)
        {
            var usager = await _context.Usagers.FindAsync(id);
            if (usager == null)
            {
                return;
            }

            usager.Nom = model.Usager.Nom;
            usager.Prenom = model.Usager.Prenom;
            usager.Courriel = model.Usager.Courriel;
            usager.Statut = model.Usager.Statut;
            usager.NumeroAbonne = model.Usager.NumeroAbonne;

            var usagerAdresse = await _context.UsagerAdresses
                .Include(ua => ua.Adresse)
                .FirstOrDefaultAsync(ua => ua.UsagerID == id && ua.TypeAdresse == model.TypeAdresse);

            if (usagerAdresse?.Adresse != null)
            {
                usagerAdresse.Adresse.Rue = model.Adresse.Rue;
                usagerAdresse.Adresse.Ville = model.Adresse.Ville;
                usagerAdresse.Adresse.Province = model.Adresse.Province;
                usagerAdresse.Adresse.CodePostale = model.Adresse.CodePostale;
            }
            else
            {
                _context.UsagerAdresses.Add(new UsagerAdresse
                {
                    UsagerID = id,
                    Adresse = model.Adresse,
                    TypeAdresse = model.TypeAdresse
                });
            }

            await _context.SaveChangesAsync();
        }
    }
}
