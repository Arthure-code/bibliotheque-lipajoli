using BibliothequeLIPAJOLI.Interfaces;
using BibliothequeLIPAJOLI.Models;
using Microsoft.EntityFrameworkCore;

namespace BibliothequeLIPAJOLI.Data
{
    public class UsagerDepot : IUsagerDepot
    {
        private readonly BibliothequeLipajoliContext _context;

        public UsagerDepot(BibliothequeLipajoliContext context)
        {
            _context = context;
        }

        public async Task<List<Usager>> ListerAsync()
        {
            return await _context.Usagers
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Usager?> ObtenirAvecSesLiensAsync(int identifiantUsager)
        {
            return await _context.Usagers
                .Include(u => u.Emprunts)
                .Include(u => u.UsagerAdresses)
                    .ThenInclude(ua => ua.Adresse)
                .AsSplitQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.ID == identifiantUsager);
        }

        public async Task<Usager?> ObtenirAsync(int identifiantUsager)
        {
            return await _context.Usagers.FindAsync(identifiantUsager);
        }

        public async Task<List<Usager>> ChercherAsync(string texteCherche)
        {
            return await _context.Usagers
                .Where(u => u.Nom.Contains(texteCherche) || u.Prenom.Contains(texteCherche))
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<UsagerAdresse?> ObtenirLAdresseAsync(int identifiantUsager, TypeAdresse? typeAdresse)
        {
            return await _context.UsagerAdresses
                .Include(ua => ua.Adresse)
                .FirstOrDefaultAsync(ua => ua.UsagerID == identifiantUsager
                    && ua.TypeAdresse == typeAdresse);
        }

        public void AjouterUneAdresse(UsagerAdresse lien)
        {
            _context.UsagerAdresses.Add(lien);
        }

        public void Supprimer(Usager usager)
        {
            _context.Usagers.Remove(usager);
        }

        public async Task EnregistrerAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
