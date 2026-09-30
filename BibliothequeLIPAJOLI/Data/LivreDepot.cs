using BibliothequeLIPAJOLI.Interfaces;
using BibliothequeLIPAJOLI.Models;
using Microsoft.EntityFrameworkCore;

namespace BibliothequeLIPAJOLI.Data
{
    public class LivreDepot : ILivreDepot
    {
        private readonly BibliothequeLipajoliContext _context;

        public LivreDepot(BibliothequeLipajoliContext context)
        {
            _context = context;
        }

        public async Task<Livre?> ObtenirAvecSesLiensAsync(int identifiantLivre)
        {
            return await AvecTousSesLiens(_context.Livres)
                .FirstOrDefaultAsync(l => l.LivreID == identifiantLivre);
        }

        public async Task<Livre?> ObtenirPourModificationAsync(int identifiantLivre)
        {
            return await _context.Livres
                .Include(l => l.Redactions)
                .Include(l => l.Editions).ThenInclude(e => e.Exemplaires)
                .AsSplitQuery()
                .FirstOrDefaultAsync(l => l.LivreID == identifiantLivre);
        }

        public async Task<Livre?> ObtenirAsync(int identifiantLivre)
        {
            return await _context.Livres.FindAsync(identifiantLivre);
        }

        public async Task<List<Livre>> ChercherAsync(string? titreCherche, IReadOnlyCollection<int> auteurs,
            int? categorieId)
        {
            ArgumentNullException.ThrowIfNull(auteurs);

            IQueryable<Livre> requete = _context.Livres;

            if (!string.IsNullOrWhiteSpace(titreCherche))
            {
                // Like ignore la casse, Contains non.
                requete = requete.Where(l =>
                    (l.Titre != null && EF.Functions.Like(l.Titre, $"%{titreCherche}%"))
                    || l.Redactions.Any(r => auteurs.Contains(r.AuteurID)));
            }

            if (categorieId.HasValue)
            {
                requete = requete.Where(l => l.CategorieID == categorieId.Value);
            }

            return await AvecTousSesLiens(requete).ToListAsync();
        }

        public async Task<List<string?>> ListerLesCodesCommencantParAsync(string prefixe)
        {
            return await _context.Livres
                .Where(l => l.Code != null && l.Code.StartsWith(prefixe))
                .Select(l => l.Code)
                .ToListAsync();
        }

        public void Ajouter(Livre livre)
        {
            _context.Livres.Add(livre);
        }

        public void RemplacerLesValeurs(Livre livre, Livre valeurs)
        {
            _context.Entry(livre).CurrentValues.SetValues(valeurs);
        }

        public void Supprimer(Livre livre)
        {
            _context.Livres.Remove(livre);
        }

        public async Task EnregistrerAsync()
        {
            await _context.SaveChangesAsync();
        }

        private static IQueryable<Livre> AvecTousSesLiens(IQueryable<Livre> requete)
        {
            return requete
                .Include(l => l.Redactions)
                .Include(l => l.Editions).ThenInclude(e => e.Exemplaires)
                    .ThenInclude(ex => ex.Emprunts)
                .AsSplitQuery();
        }
    }
}
